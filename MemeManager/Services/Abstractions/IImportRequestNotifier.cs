using System;
using System.Collections.Generic;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IImportRequestNotifier
{
    event EventHandler<ImportRequestEventArgs> ImportRequest;
    event EventHandler<GenerateThumbnailsRequestEventArgs> GenerateThumbnailsRequest;
    event EventHandler<SetThumbnailsRequestEventArgs> SetThumbnailsRequest;

    void SendImportRequest(string basePath, string[] memePaths);
    void SendGenerateThumbnailsRequest(IEnumerable<Meme> memes);
    void SendSetThumbnailsRequest(IEnumerable<(Meme, string?)> memesAndThumbnailPaths);
}

public class ImportRequestEventArgs : EventArgs
{
    public ImportRequestEventArgs(string basePath, string[] memePaths)
    {
        BasePath = basePath;
        MemePaths = memePaths;
    }

    public string BasePath { get; }
    public string[] MemePaths { get; }
}

public class GenerateThumbnailsRequestEventArgs : EventArgs
{
    public GenerateThumbnailsRequestEventArgs(IEnumerable<Meme> memes)
    {
        Memes = memes;
    }

    public IEnumerable<Meme> Memes { get; }
}

public class SetThumbnailsRequestEventArgs : EventArgs
{
    public SetThumbnailsRequestEventArgs(IEnumerable<(Meme, string?)> memesAndThumbnailPaths)
    {
        MemesAndThumbnailPaths = memesAndThumbnailPaths;
    }

    public IEnumerable<(Meme, string?)> MemesAndThumbnailPaths { get; }
}
