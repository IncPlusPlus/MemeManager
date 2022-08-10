using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using MemeManager.Persistence;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class MemesListViewModel : ViewModelBase, IMemesListViewModel
{
    public MemesListViewModel(IFilterObserverService filterObserverService)
    {
        RxApp.MainThreadScheduler.Schedule(LoadMemes);
    }

    public ObservableCollection<FileViewModel> Memes { get; } = new();

    private async void LoadMemes()
    {
        var memes = await FileViewModel.LoadCached();

        foreach (var meme in memes)
        {
            Memes.Add(meme);
        }
        
        LoadThumbnails();
    }

    private async void LoadThumbnails()
    {
        foreach (var meme in Memes.ToList())
        {
            await meme.LoadThumbnail();
        }
    }
}