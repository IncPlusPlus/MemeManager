using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MemeManager.Persistence.Entity;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace MemeManager.ViewModels;

public class MemeListViewModel : ViewModelBase
{
    public MemeListViewModel(IEnumerable<Meme> memes)
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