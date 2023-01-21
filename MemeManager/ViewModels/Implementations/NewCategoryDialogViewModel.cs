using System;
using System.Windows.Input;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class NewCategoryDialogViewModel : ViewModelBase, INewCategoryDialogViewModel
{
    private bool? dialogResult;
    private string text = string.Empty;

    public NewCategoryDialogViewModel()
    {
        OkCommand = ReactiveCommand.Create(Ok);
    }

    public ICommand OkCommand { get; }
    public event EventHandler? RequestClose;

    public string Text
    {
        get => text;
        set => this.RaiseAndSetIfChanged(ref text, value, nameof(Text));
    }

    public bool? DialogResult
    {
        get => dialogResult;
        set => this.RaiseAndSetIfChanged(ref dialogResult, value, nameof(DialogResult));
    }

    private void Ok()
    {
        if (!string.IsNullOrEmpty(Text))
        {
            DialogResult = true;
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Cancel()
    {
        DialogResult = false;
        RequestClose?.Invoke(this, EventArgs.Empty);
    }
}
