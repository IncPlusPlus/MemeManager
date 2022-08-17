using MemeManager.Services;
using MemeManager.ViewModels.Interfaces;

namespace MemeManager.ViewModels.Implementations
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private Database db = new Database();

        public MainWindowViewModel(ICategoriesListViewModel categoriesListViewModel,
            IMemesListViewModel memesListViewModel)
        {
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
        }

        public ICategoriesListViewModel CategoriesListViewModel { get; }
        public IMemesListViewModel MemesListViewModel { get; }

        public MemesListViewModel List { get; }
    }
}