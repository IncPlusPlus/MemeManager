using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;
using IOPath = System.IO.Path;

namespace MemeManager.ViewModels.Implementations;

public class ImportFolderDialogViewModel : ViewModelBase, IImportFolderDialogViewModel
{
    private readonly IDialogService _dialogService;
    private bool? _dialogResult;
    private string? _path;

    public ImportFolderDialogViewModel(IDialogService dialogService)
    {
        this._dialogService = dialogService;
        OkCommand = ReactiveCommand.Create(Ok);
        BrowseFolderCommand = ReactiveCommand.Create(OpenFolderAsync);
    }

    public ICommand BrowseFolderCommand { get; }

    public ICommand OkCommand { get; }

    public string? Path
    {
        get => _path;
        private set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    public event EventHandler? RequestClose;

    public bool? DialogResult
    {
        get => _dialogResult;
        set => this.RaiseAndSetIfChanged(ref _dialogResult, value);
    }

    private async Task OpenFolderAsync()
    {
        var settings = new OpenFolderDialogSettings
        {
            Title = "This is a description",
            InitialDirectory = IOPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!
        };

        var result = await _dialogService.ShowOpenFolderDialogAsync(this, settings);
        Path = result;
    }

    private void Ok()
    {
        if (!string.IsNullOrEmpty(Path))
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
