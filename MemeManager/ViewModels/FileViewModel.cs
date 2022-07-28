using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using MemeManager.Persistence.Entity;
using ReactiveUI;

namespace MemeManager.ViewModels;

public class FileViewModel : ViewModelBase
{
    private Bitmap? _thumbnail;
    private readonly Meme _meme;

    public FileViewModel(Meme meme)
    {
        _meme = meme;
    }

    public int Id => _meme.Id;
    public string Name => _meme.Name;

    public Bitmap? Thumbnail
    {
        get => _thumbnail;
        private set => this.RaiseAndSetIfChanged(ref _thumbnail, value);
    }

    public async Task LoadThumbnail()
    {
        await using (var imageStream = await _meme.LoadThumbnailAsync())
        {
            Thumbnail = await Task.Run(() => Bitmap.DecodeToWidth(imageStream, 400));
        }
    }
    
    public static async Task<IEnumerable<FileViewModel>> LoadCached()
    {
        return (await Meme.LoadCachedAsync()).Select(x => new FileViewModel(x));
    }
}