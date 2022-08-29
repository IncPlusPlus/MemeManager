using Avalonia.Controls;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Interfaces;

namespace MemeManager.ViewModels.Implementations
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly LayoutConfiguration _layoutConfig;

        public MainWindowViewModel(ISearchbarViewModel searchbarViewModel,
            ICategoriesListViewModel categoriesListViewModel,
            IMemesListViewModel memesListViewModel, LayoutConfiguration layoutConfig)
        {
            SearchbarViewModel = searchbarViewModel;
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
            _layoutConfig = layoutConfig;
        }

        public GridLength LeftPanelWidth
        {
            get => _layoutConfig.LeftPanelWidth;
            set => _layoutConfig.LeftPanelWidth = value;
        }
        public ISearchbarViewModel SearchbarViewModel { get; }
        public ICategoriesListViewModel CategoriesListViewModel { get; }
        public IMemesListViewModel MemesListViewModel { get; }
    }
}
