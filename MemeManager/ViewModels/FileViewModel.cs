using System.Reactive;
using MemeManager.Persistence.Entity;
using ReactiveUI;

namespace MemeManager.ViewModels;

public class FileViewModel : ViewModelBase
{
    private readonly Meme _meme;
    private string? _thumbnail;

    public FileViewModel(Meme meme)
    {
        _meme = meme;
        _thumbnail = meme.CachedThumbnailPath;
        OpenRecentCommand = ReactiveCommand.Create<object>(OpenRecent);
    }

    public int Id => _meme.Id;
    public string Name => _meme.Name;

    internal Meme Meme => _meme;

    public string? Thumbnail
    {
        get => _thumbnail;
        private set => this.RaiseAndSetIfChanged(ref _thumbnail, value);
    }

    public ReactiveCommand<object, Unit> OpenRecentCommand { get; }
    public void OpenRecent(object path)
    {
        System.Diagnostics.Debug.WriteLine($"Open recent: {path}");
    }
}
