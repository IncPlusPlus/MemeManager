using System.Windows.Input;
using Avalonia.Controls;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly IImportService _importService;
        private readonly LayoutConfiguration _layoutConfig;

        public MainWindowViewModel(ISearchbarViewModel searchbarViewModel,
            ICategoriesListViewModel categoriesListViewModel,
            IMemesListViewModel memesListViewModel, LayoutConfiguration layoutConfig, IImportService importService)
        {
            SearchbarViewModel = searchbarViewModel;
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
            _layoutConfig = layoutConfig;
            _importService = importService;
            ImportCommand = ReactiveCommand.Create(Import);
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

        private void Import()
        {
            _importService.ImportFromDirectory(@"C:\Users\reach\Downloads\MyMemes");
        }
    }
}
