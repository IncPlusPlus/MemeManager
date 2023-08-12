using System;
using System.Collections.Generic;
using MemeManager.Extensions;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;

namespace MemeManager.Services.Implementations;

public class ImportRequestNotifier : IImportRequestNotifier
{
    public event EventHandler<ImportRequestEventArgs>? ImportRequest;
    public event EventHandler<GenerateThumbnailsRequestEventArgs>? GenerateThumbnailsRequest;
    public event EventHandler<SetThumbnailsRequestEventArgs>? SetThumbnailsRequest;

    public void SendImportRequest(string basePath)
    {
        ImportRequest.Raise(this, new ImportRequestEventArgs(basePath));
    }

    public void SendGenerateThumbnailsRequest(IEnumerable<Meme> memes)
    {
        GenerateThumbnailsRequest.Raise(this, new GenerateThumbnailsRequestEventArgs(memes));
    }

    public void SendSetThumbnailsRequest(IEnumerable<(Meme, string?)> memesAndThumbnailPaths)
    {
        SetThumbnailsRequest.Raise(this, new SetThumbnailsRequestEventArgs(memesAndThumbnailPaths));
    }
}
