using MemeManager.Services;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private Database db= new Database();
        public ICategoriesListViewModel CategoriesListViewModel { get; }
        public IMemesListViewModel MemesListViewModel { get; }

        public MainWindowViewModel(ICategoriesListViewModel categoriesListViewModel, IMemesListViewModel memesListViewModel)
        {
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
        }
        
        public MemesListViewModel List { get; }
    }
}
