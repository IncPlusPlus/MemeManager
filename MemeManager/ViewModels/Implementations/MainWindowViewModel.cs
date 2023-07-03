using System;
using System.Reactive;
using System.Reactive.Linq;
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
        private readonly IObservable<EventPattern<GenerateThumbnailsRequestEventArgs>> _generateThumbnailsRequestObservable;
        private readonly IObservable<EventPattern<ImportRequestEventArgs>> _importRequestObservable;
        private readonly IImportService _importService;
        private readonly LayoutConfiguration _layoutConfig;
        private readonly IObservable<EventPattern<SetThumbnailsRequestEventArgs>> _setThumbnailsRequestObservable;

        public MainWindowViewModel(IDialogService dialogService, ISearchbarViewModel searchbarViewModel,
            IStatusBarViewModel statusBarViewModel, ICategoriesListViewModel categoriesListViewModel,
            IMemesListViewModel memesListViewModel, LayoutConfiguration layoutConfig, IImportService importService,
            IImportRequestNotifier importRequestNotifier)
        {
            _dialogService = dialogService;
            SearchbarViewModel = searchbarViewModel;
            StatusBarViewModel = statusBarViewModel;
            CategoriesListViewModel = categoriesListViewModel;
            MemesListViewModel = memesListViewModel;
            _layoutConfig = layoutConfig;
            _importService = importService;
            ImportCommand = ReactiveCommand.CreateFromTask(OpenImportDialog);

            #region Terrible, awful hack to allow for a background Task to do all EF operations on the UI thread to avoid DbContext threading issues

            _importRequestObservable =
                Observable.FromEventPattern<EventHandler<ImportRequestEventArgs>, ImportRequestEventArgs>(
                    handler => importRequestNotifier.ImportRequest += handler,
                    handler => importRequestNotifier.ImportRequest -= handler);
            _generateThumbnailsRequestObservable =
                Observable
                    .FromEventPattern<EventHandler<GenerateThumbnailsRequestEventArgs>,
                        GenerateThumbnailsRequestEventArgs>(
                        handler => importRequestNotifier.GenerateThumbnailsRequest += handler,
                        handler => importRequestNotifier.GenerateThumbnailsRequest -= handler);
            _setThumbnailsRequestObservable =
                Observable.FromEventPattern<EventHandler<SetThumbnailsRequestEventArgs>, SetThumbnailsRequestEventArgs>(
                    handler => importRequestNotifier.SetThumbnailsRequest += handler,
                    handler => importRequestNotifier.SetThumbnailsRequest -= handler);

            this.WhenAnyObservable(x => x._importRequestObservable)
                .Select(x => x.EventArgs)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(x => importService.ImportFromPaths(x.BasePath));

            this.WhenAnyObservable(x => x._generateThumbnailsRequestObservable)
                .Select(x => x.EventArgs)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .Subscribe(x => importService.GenerateThumbnails(x.Memes));

            this.WhenAnyObservable(x => x._setThumbnailsRequestObservable)
                .Select(x => x.EventArgs)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => importService.SetThumbnails(x.MemesAndThumbnailPaths));
            #endregion
        }

        public ICommand ImportCommand { get; }

        public GridLength LeftPanelWidth
        {
            get => _layoutConfig.LeftPanelWidth;
            set => _layoutConfig.LeftPanelWidth = value;
        }

        public ISearchbarViewModel SearchbarViewModel { get; }
        public IStatusBarViewModel StatusBarViewModel { get; }
        public ICategoriesListViewModel CategoriesListViewModel { get; }
        public IMemesListViewModel MemesListViewModel { get; }

        private async Task OpenImportDialog()
        {
            var dialogViewModel = _dialogService.CreateViewModel<IImportFolderDialogViewModel>();
            var success = await _dialogService.ShowDialogAsync<ImportFolderDialog>(this, dialogViewModel)
                .ConfigureAwait(true);
            if (success == true && dialogViewModel.Path != null)
            {
                _importService.ImportFromDirectory(dialogViewModel.Path);
            }
        }
    }
}
