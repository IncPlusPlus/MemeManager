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
    }

    public int Id => _meme.Id;
    public string Name => _meme.Name;

    public string? Thumbnail
    {
        get => _thumbnail;
        private set => this.RaiseAndSetIfChanged(ref _thumbnail, value);
    }
}