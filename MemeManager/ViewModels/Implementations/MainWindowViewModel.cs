using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly IDialogService _dialogService;
        private readonly IImportService _importService;
        private readonly LayoutConfiguration _layoutConfig;

        public MainWindowViewModel(IDialogService dialogService, ISearchbarViewModel searchbarViewModel,
            ICategoriesListViewModel categoriesListViewModel,
            IMemesListViewModel memesListViewModel, LayoutConfiguration layoutConfig, IImportService importService)
        {
            _dialogService = dialogService;
            SearchbarViewModel = searchbarViewModel;
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
            _layoutConfig = layoutConfig;
            _importService = importService;
            ImportCommand = ReactiveCommand.CreateFromTask(OpenImportDialog);
        }

        public ICommand ImportCommand { get; }

        public GridLength LeftPanelWidth
        {
            get => _layoutConfig.LeftPanelWidth;
            set => _layoutConfig.LeftPanelWidth = value;
        }

        public ISearchbarViewModel SearchbarViewModel { get; }
        public ICategoriesListViewModel CategoriesListViewModel { get; }
        public IMemesListViewModel MemesListViewModel { get; }

        private async Task OpenImportDialog()
        {
            var dialogViewModel = _dialogService.CreateViewModel<ISelectFolderDialogViewModel>();
            var success = await _dialogService.ShowDialogAsync<SelectFolderDialog>(this, dialogViewModel).ConfigureAwait(true);
            if (success == true)
            {
                _importService.ImportFromDirectory(dialogViewModel.Path);
            }
        }
    }
}
