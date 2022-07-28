using MemeManager.Services;
using ReactiveUI;

namespace MemeManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Database db= new Database();
        ViewModelBase content;
        public string Greeting => "Welcome to Avalonia!";

        public MainWindowViewModel()
        {
            Content = List = new MemeListViewModel(db.GetMemes());
        }

        public ViewModelBase Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }
        
        public MemeListViewModel List { get; }
    }
}
