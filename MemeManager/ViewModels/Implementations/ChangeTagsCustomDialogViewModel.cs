using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Controls;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using OneOf;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class ChangeTagsCustomDialogViewModel : ViewModelBase, IChangeTagsCustomDialogViewModel
{
    private bool? _dialogResult;
    private bool _hasPendingNewTags;
    private bool _hasTagsPendingRemoval;
    private AvaloniaList<string> _pendingNewTags = new();
    private string[] _pendingRemovalTags = Array.Empty<string>();

    private Queue<(TagAction ta, OneOf<Tag, string> tag)> _tagActionQueue;
    private IEnumerable<Meme> _targetMemes;

    public ChangeTagsCustomDialogViewModel(IMemeService memeService, ITagService tagService)
    {
        MemeService = memeService;
        TagService = tagService;
        OkCommand = ReactiveCommand.Create(Ok);
        AddTagCommand = ReactiveCommand.Create<AutoCompleteBox>(HandleEnter);
        CleanUpClass();
    }

    public ICommand OkCommand { get; }
    public ReactiveCommand<AutoCompleteBox, Unit> AddTagCommand { get; }

    private IMemeService MemeService { get; set; }

    private ITagService TagService { get; set; }

    public IEnumerable<Tag> ExistingTags { get; set; }

    public Tag[] Tags { get; private set; } = Array.Empty<Tag>();

    public bool HasPendingNewTags
    {
        get => _hasPendingNewTags;
        set => this.RaiseAndSetIfChanged(ref _hasPendingNewTags, value, nameof(HasPendingNewTags));
    }

    public AvaloniaList<string> PendingNewTags
    {
        get => _pendingNewTags;
        set
        {
            _pendingNewTags.Clear();
            _pendingNewTags.AddRange(value);
            HasPendingNewTags = _pendingNewTags.Count > 0;
        }
    }

    public bool HasTagsPendingRemoval
    {
        get => _hasTagsPendingRemoval;
        set => this.RaiseAndSetIfChanged(ref _hasTagsPendingRemoval, value, nameof(HasTagsPendingRemoval));
    }

    public string[] PendingRemovalTags
    {
        get => _pendingRemovalTags;
        set
        {
            this.RaiseAndSetIfChanged(ref _pendingRemovalTags, value, nameof(PendingRemovalTags));
            HasTagsPendingRemoval = _pendingRemovalTags.Length > 0;
        }
    }

    public bool IsMultipleMemes { get => TargetMemes.Count() > 1; }
    public event EventHandler? RequestClose;

    public IEnumerable<Meme> TargetMemes
    {
        get => _targetMemes;
        set
        {
            _targetMemes = value;
            // Find the common tags between the memes if multiple were selected
            ExistingTags = value.Select(meme => meme.Tags).Cast<IEnumerable<Tag>>().Aggregate((x, y) => x.Intersect(y));
        }
    }

    public bool? DialogResult
    {
        get => _dialogResult;
        private set => this.RaiseAndSetIfChanged(ref _dialogResult, value, nameof(DialogResult));
    }

    private void AddTag(OneOf<Tag, string> additionalTag)
    {
        if (_tagActionQueue.Contains((TagAction.Add, additionalTag)))
            return;
        _tagActionQueue.Enqueue((TagAction.Add, additionalTag));
        UpdatePendingTagsDisplay();
    }

    private void RemoveTag(Tag tagToRemove)
    {
        if (_tagActionQueue.Contains((TagAction.Delete, tagToRemove)))
            return;
        _tagActionQueue.Enqueue((TagAction.Delete, tagToRemove));
        UpdatePendingTagsDisplay();
    }

    private void RemovePendingNewTag(string tagName)
    {
        // Recreate the queue with all action entries except for the specific new tag action we're undoing
        _tagActionQueue = new Queue<(TagAction ta, OneOf<Tag, string> tag)>(_tagActionQueue.Where(tuple =>
            !(tuple.ta == TagAction.Add
              && tuple.tag
                  .Match(
                      t => t.Name,
                      s => s).Equals(tagName))));
        UpdatePendingTagsDisplay();
    }

    private void CancelTagRemoval(string tagThatWouldBeRemoved)
    {
        // Recreate the queue with all action entries except for the tag removal we're undoing
        _tagActionQueue = new Queue<(TagAction ta, OneOf<Tag, string> tag)>(_tagActionQueue.Where(tuple =>
            !(tuple.ta == TagAction.Delete
              && tuple.tag
                  .Match(
                      t => t.Name,
                      s => s).Equals(tagThatWouldBeRemoved))));
        UpdatePendingTagsDisplay();
    }

    private void HandleEnter(AutoCompleteBox autoCompleteBox)
    {
        if (autoCompleteBox.SelectedItem is Tag tag)
        {
            AddTag(tag);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(autoCompleteBox.Text))
            {
                AddTag(autoCompleteBox.Text);
            }
        }

        // Clears the text in the text box after the user enters the tag they want to add
        autoCompleteBox.Text = "";
    }

    private void UpdatePendingTagsDisplay()
    {
        PendingNewTags = new AvaloniaList<string>(_tagActionQueue.Where(entry => entry.ta == TagAction.Add).Select(
            entry =>
                entry.tag.Match(
                    t => t.Name,
                    s => s)
        ).ToArray());
        PendingRemovalTags = _tagActionQueue.Where(entry => entry.ta == TagAction.Delete).Select(entry =>
            entry.tag.Match(
                t => t.Name,
                s => s)
        ).ToArray();
    }

    private void Ok()
    {
        ApplyTagActions();
        DialogResult = true;
        CleanUpClass();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public void Cancel()
    {
        DialogResult = false;
        CleanUpClass();
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyTagActions()
    {
        while (_tagActionQueue.Any())
        {
            var (ta, tagOrString) = _tagActionQueue.Dequeue();
            // The current tag to either add or remove from the selected memes
            var tag = tagOrString.Match(
                // If already a Tag object, return the instance
                t => t,
                // If it's just a string, we need to create a new Tag in the database before applying it to the memes
                s => TagService.Create(new Tag() { Name = s }));
            switch (ta)
            {
                case TagAction.Add:
                    foreach (var targetMeme in TargetMemes)
                    {
                        MemeService.AddTag(targetMeme, tag);
                    }

                    break;
                case TagAction.Delete:
                    foreach (var targetMeme in TargetMemes)
                    {
                        MemeService.RemoveTag(targetMeme, tag);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException("The provided " + nameof(ta) + " is not valid.");
            }
        }
    }

    /// <summary>
    /// Actions to clean up the class after use. This is horrible practice but we have to use this same INSTANCE of this
    /// ViewModel class every time a request to change the tags of a meme is made. I'd love to use a factory pattern to
    /// create a new instance of this class each time. However, with the way that dialogs are created, the API doesn't
    /// allow for that. We have to provide a ViewModel class. This method should be called ALWAYS before closing the
    /// dialog so that any necessary cleanup can occur and no funny business happens the next time it's opened.
    /// </summary>
    private void CleanUpClass()
    {
        _tagActionQueue = new Queue<(TagAction ta, OneOf<Tag, string> tag)>();
        PendingRemovalTags = Array.Empty<string>();
        PendingNewTags = new AvaloniaList<string>();
        Tags = TagService.GetAll().ToArray();
    }

    private enum TagAction
    {
        Add,
        Delete,
    }
}
