using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace MemeManager.Persistence.Entity;

public partial class Meme : IRealmObject
{
    public enum FileMediaType
    {
        Image,
        Video,
        Gif,
        Audio,
        Other
    }

    [PrimaryKey]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();

    /// <summary>
    /// The name of the meme. If a meme hasn't been explicitly named, it'll just be displayed with its file name.
    /// </summary>
    public string Name { set; get; }

    public string Path { get; set; }

    public string? CachedThumbnailPath { get; set; }

    // TODO: I might want to make this default to the file creation date when the library is first initialized
    public DateTimeOffset TimeAdded { get; set; }

    /// <summary>
    /// A tag is for labelling a meme with all relevant terms
    /// </summary>
    public IList<Tag> Tags { get; } = null!;

    public Category? Category { get; set; }

    // TODO: Change AdditionalTerms to Keywords
    public string AdditionalTerms { get; set; }
    // Seems that Atlas can't handle enums. Their AI suggested this might work. We'll see if that can be trusted...
    private string _MediaType { get; set; }
    public FileMediaType MediaType
    {
        get => Enum.Parse<FileMediaType>(_MediaType);
        set => _MediaType = value.ToString();
    }

    /// <summary>
    /// True if the user has already added their desired name, tags, and category to this meme. False if this file
    /// was recently discovered and hasn't been reviewed by the user yet.
    /// </summary>
    public bool HasBeenCategorized { get; set; }
}
