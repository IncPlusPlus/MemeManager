using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Binding;
using MemeManager.DependencyInjection;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.Models;

public class CategoryTreeNodeModel : ReactiveObject
{
    private Category _category;
    private ICategoryService _categoryService;
    private ObservableCollection<CategoryTreeNodeModel>? _children;
    private bool _hasChildren;
    private bool _isExpanded;
    private ILogger _log = Locator.Current.GetRequiredService<ILogger>();
    private int _memeCount;
    private string _name;

    public CategoryTreeNodeModel(Category category, ICategoryService categoryService)
    {
        _category = category;
        _name = category.Name;
        _categoryService = categoryService;
        _isExpanded = false;
        _hasChildren = category.Children?.Count > 0;
        _memeCount = category.Memes.Count;

        // Maybe bind this to a SourceCache instead
        this.Category.Children.CollectionChanged += (sender, args) =>
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add when args.NewItems?[0] is Category newCategory:
                    _children?.Add(new CategoryTreeNodeModel(newCategory, _categoryService));
                    break;
                case NotifyCollectionChangedAction.Remove when args.OldItems?[0] is Category oldCategory:
                    {
                        var nodeToRemove = _children?.Where(node => node._category.Id == oldCategory.Id).First();
                        if (nodeToRemove == null)
                        {
                            _log.LogError("Tried to remove a category with id {OldId} and name {OldName} from CategoryTreeNodeModel but couldn't find the node with a matching ID", oldCategory.Id, oldCategory.Id);
                        }
                        else
                        { _children?.Remove(nodeToRemove); }
                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    _log.LogWarning("Encountered unhandled NotifyCollectionChangedAction {Action} in CategoryTreeNodeModel", args.Action);
                    break;
            }
        };

        // Handles when a category is renamed
        this.WhenValueChanged(x => x.Name)
            // .Throttle(TimeSpan.FromMilliseconds(5000))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Where(s => s != null && !s.Equals(Category.Name))
            .Subscribe(s =>
            {
                _categoryService.Rename(Category, s ?? string.Empty);
            });

        Category.Memes.CollectionChanged += MemesChanged;
    }

    public bool IsRoot => Category.Parent == null;

    public Category Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public string Name
    {
        get => _name;
        private set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int MemeCount
    {
        get => _memeCount;
        private set => this.RaiseAndSetIfChanged(ref _memeCount, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        private set => this.RaiseAndSetIfChanged(ref _hasChildren, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public IReadOnlyList<CategoryTreeNodeModel> Children => _children ??= LoadChildren();


    private void MemesChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
        if (sender == null)
        {
            return;
        }

        var collection = (ObservableCollection<Meme>)sender;
        /*
         * For some reason, notifyCollectionChangedEventArgs.NewItems is always yielding null. I noticed that the sender
         * is an instance of the ObservableCollection itself. Because that object has the updated collection, I can get
         * the Count from there instead of from notifyCollectionChangedEventArgs.NewItems.
         */
        MemeCount = collection.Count;
    }

    private ObservableCollection<CategoryTreeNodeModel> LoadChildren()
    {
        var options = new EnumerationOptions { IgnoreInaccessible = true };
        var result = new ObservableCollection<CategoryTreeNodeModel>();

        Category.Children?.ForEach(childCategory =>
            result.Add(new CategoryTreeNodeModel(childCategory, _categoryService)));

        if (result.Count == 0)
            HasChildren = false;

        return result;
    }

    public static Comparison<CategoryTreeNodeModel?> SortAscending<T>(Func<CategoryTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return -1;
            else if (y is null)
                return 1;
            else
                return 1;
        };
    }

    public static Comparison<CategoryTreeNodeModel?> SortDescending<T>(Func<CategoryTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return 1;
            else if (y is null)
                return -1;
            else
                return 1;
        };
    }
}

public static class EnumerableExtensions
{
    /// <summary>
    /// One would hope that C#'s foreach loops would just not iterate if the collection is null but no. So now I need
    /// this stupid thing. Thanks to https://stackoverflow.com/a/6535813/1687436. 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public static void ForEach<T>(this IEnumerable<T>? self, Action<T> action)
    {
        if (self != null)
        {
            foreach (var element in self)
            {
                action(element);
            }
        }
    }
}
