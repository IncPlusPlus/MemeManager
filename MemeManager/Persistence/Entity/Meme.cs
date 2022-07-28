using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EFGetStarted.Persistence;
using MemeManager.Services;
using Microsoft.EntityFrameworkCore;

namespace MemeManager.Persistence.Entity;

public class Meme
{
    public int Id { get; set; }
    /// <summary>
    /// The name of the meme. If a meme hasn't been explicitly named, it'll just be displayed with its file name.
    /// </summary>
    public string Name { set; get; }
    public string Path { get; set; }
    public string? CachedThumbnailPath { get; set; }
    // TODO: I might want to make this default to the file creation date when the library is first initialized
    public DateTime TimeAdded { get; set; }
    /// <summary>
    /// A tag is for labelling a meme with all relevant terms
    /// </summary>
    public virtual ICollection<Tag> Tags { get; set; }
    public virtual Category Category { get; set; }
    public string AdditionalTerms { get; set; }
    public FileMediaType MediaType { get; set; }

    public enum FileMediaType
    {
        Image,
        Video,
        Gif,
        Other
    }

    public async Task<Stream> LoadThumbnailAsync()
    {
        if (File.Exists(CachedThumbnailPath))
        {
            return File.OpenRead(CachedThumbnailPath);
        }
        else
        {
            throw new Exception("Thumbnail not found");
        }
    }

    public static async Task<IEnumerable<Meme>> LoadCachedAsync()
    {
        return await new Database().GetMemesAsync();
        // return await new MemeManagerContext().Memes.ToListAsync();
    }
}