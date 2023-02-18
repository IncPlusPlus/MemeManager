﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MemeManager.Services.Implementations;

// TODO: This class's performance could be further improved by performing all the read operations all at once instead
// reading and then writing the thumbnail for each meme.
public class ImportService : IImportService
{
    // TODO: If I ever get around to adding tests, I should add a test that makes sure that the union of these file extensions arrays is an empty array
    // https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types
    public static readonly string[] s_imageExtensions = new[]
    {
        "jpg", "jpeg", "jfif", "pjpeg", "pjp", "png", "webp", "tif", "tiff", "bmp", "heic", "heif", "avif"
    };

    private static readonly string[] s_animatedImageExtensions = new[] { "gif", "apng" };

    private static readonly string[] s_videoExtensions = new[]
    {
        "mp4", "avi", "mov", "wmv", "mkv", "webm", "flv", "f4v", "swf", "avchd", "ass", "ts", "3gp"
    };

    private static readonly string[] s_audioExtensions = new[] { "wav", "flac", "ogg", "mp3", "alac", "aac" };
    private readonly ICategoryService _categoryService;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;
    private readonly IMemeService _memeService;
    private readonly ITagService _tagService;
    private int _current;

    private int _total;


    public ImportService(ILogger logger, IDbChangeNotifier dbChangeNotifier, IMemeService memeService,
        ICategoryService categoryService, ITagService tagService)
    {
        var separateContext = new MemeManagerContext();
        _log = logger;
        _dbChangeNotifier = dbChangeNotifier;
        _memeService = memeService.NewInstance(separateContext);
        _categoryService = categoryService.NewInstance(separateContext);
        _tagService = tagService.NewInstance(separateContext);
    }

    public double Progress
    {
        get
        {
            if (_total == 0)
                return 0;
            return (double)_current / _total;
        }
    }

    // TODO: Expand this with options
    /*
     * Potential options:
     * - Include ALL items or just ones with known extensions
     * - Exclude certain directories
     * - Rough tag recognition based on comma separated values inside parenthesis
     * - Additional file extensions to include and what media type they are (in case I'm missing any in the extensions lists)
     * - Skip generating thumbnails
     * - Files to skip (a regexp for a file name or names)
     */
    public IEnumerable<Meme> ImportFromDirectory(string path)
    {
        var importedMemes = new List<Meme>();
        try
        {
            _log.LogInformation("IMPORT OF MEMES FROM PATH {MemesPath} STARTING!", path);
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            importedMemes = new List<Meme>(files.Length);
            foreach (var file in files)
            {
                // Skip memes we already know about
                if (_memeService.GetByPath(file) != null)
                    continue;
                var fileInfo = new FileInfo(file);
                var memeType = ClassifyFile(fileInfo);
                var memeCategory = CreateCategoryIfNotExists(path, file);
                // TODO: Maybe add a method to MemeService that allows for adding a list of Meme entities to the DB to avoid frequent DB writes and UI updates (because of the notifier being fired for every new meme)
                importedMemes.Add(_memeService.Create(new Meme()
                {
                    Path = file,
                    Name = fileInfo.Name,
                    MediaType = memeType,
                    Category = memeCategory,
                    TimeAdded = DateTime.Now,
                    AdditionalTerms = ""
                }));
            }
            _log.LogInformation("IMPORT OF MEMES FROM PATH {MemesPath} SUCCEEDED!", path);
            return importedMemes;
        }
        catch (Exception e)
        {
            _log.LogError(e, "IMPORT OF MEMES FROM PATH {MemesPath} FAILED!", path);
            return importedMemes;
        }
    }

    public async Task GenerateThumbnails(IEnumerable<Meme> memes)
    {
        _log.LogInformation("THUMBNAIL GENERATION OF MEMES STARTING!");
        Stopwatch watch = new Stopwatch();
        watch.Start();
        // Maybe some day I'll use System.IProgress for this.

        // TODO: See if this can be parallelized into a threadpool or something so it doesn't take forever to load
        // thumbnails one-by-one in a big library.
        // Try 1: Not parallel. Kinda slow
        // foreach (var meme in memes)
        // {
        //     try
        //     {
        //         var thumbnailPath = meme.MediaType switch
        //         {
        //             Meme.FileMediaType.Image => await GenerateThumbnailForImage(meme),
        //             Meme.FileMediaType.Video => await GenerateThumbnailForVideo(meme),
        //             Meme.FileMediaType.Gif => await GenerateThumbnailForGif(meme),
        //             _ => null
        //         };
        //         _memeService.SetThumbnailPath(meme, thumbnailPath);
        //     }
        //     catch (Exception e)
        //     {
        //         _log.LogError(e, "Failed to generate thumbnail for meme at path '{MemePath}'", meme.Path);
        //     }
        // }

        // Try 2: Parallel but lots of DB requests.
        var options = new ParallelOptions { MaxDegreeOfParallelism = 2 };
        var gotCount = memes.TryGetNonEnumeratedCount(out this._total);
        this._current = 0;
        var thumbnails = new ConcurrentBag<(Meme, string?)>();

        // CancellationToken is ignored for now but I might use this if I make this Task cancelable
        await Parallel.ForEachAsync(memes, options, async (meme, token) =>
        {
            try
            {
                var thumbnailPath = meme.MediaType switch
                {
                    Meme.FileMediaType.Image => await GenerateThumbnailForImage(meme),
                    Meme.FileMediaType.Video => await GenerateThumbnailForVideo(meme),
                    Meme.FileMediaType.Gif => await GenerateThumbnailForGif(meme),
                    _ => null
                };
                thumbnails.Add((meme, thumbnailPath));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to generate thumbnail for meme at path '{MemePath}'", meme.Path);
            }
            finally
            {
                Interlocked.Increment(ref this._current);
            }
        });
        watch.Stop();
        // _log.LogInformation("THUMBNAIL GENERATION OF MEMES FINISHED! Saving...");
        _log.LogInformation("Finished. Elapsed={TimeElapsed}", watch.Elapsed);
        _memeService.SetThumbnailPaths(thumbnails);
        // _log.LogInformation("Thumbnails saved!");
    }

    private async Task<string?> GenerateThumbnailForImage(Meme meme)
    {
        var memeFile = new FileInfo(meme.Path);
        var thumbnailsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData,
            System.Environment.SpecialFolderOption.DoNotVerify);
        thumbnailsDir = Path.Join(thumbnailsDir, "MemeManagerThumbnails");
        // Ensure the directory and all its parents exist.
        Directory.CreateDirectory(thumbnailsDir);
        var thumbnailPath = Path.Join(thumbnailsDir, meme.Id + "-" + memeFile.Name);

        using var image = await Image.LoadAsync(meme.Path);
        var size = image.Size();
        if (size.Height > size.Width)
        {
            image.Mutate(x => x.Resize(0, 200));
        }
        else
        {
            image.Mutate(x => x.Resize(200, 0));
        }

        await image.SaveAsync(thumbnailPath);

        return thumbnailPath;
    }

    private async Task<string?> GenerateThumbnailForGif(Meme meme)
    {
        var memeFile = new FileInfo(meme.Path);
        var thumbnailsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData,
            System.Environment.SpecialFolderOption.DoNotVerify);
        thumbnailsDir = Path.Join(thumbnailsDir, "MemeManagerThumbnails");
        // Ensure the directory and all its parents exist.
        Directory.CreateDirectory(thumbnailsDir);
        var thumbnailPath = Path.Join(thumbnailsDir, meme.Id + "-" + memeFile.Name + ".png");

        using var image = await Image.LoadAsync(meme.Path);

        if (image.Frames.Count < 1)
        {
            return null;
        }

        var frames = image.Frames;
        var firstFrame = frames.CloneFrame(0);
        var size = firstFrame.Size();
        if (size.Height > size.Width)
        {
            firstFrame.Mutate(x => x.Resize(0, 200));
        }
        else
        {
            firstFrame.Mutate(x => x.Resize(200, 0));
        }
        await firstFrame.SaveAsync(thumbnailPath);
        return thumbnailPath;
    }

    private async Task<string?> GenerateThumbnailForVideo(Meme meme)
    {
        await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official);

        var memeFile = new FileInfo(meme.Path);
        var thumbnailsDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData,
            System.Environment.SpecialFolderOption.DoNotVerify);
        thumbnailsDir = Path.Join(thumbnailsDir, "MemeManagerThumbnails");
        // Ensure the directory and all its parents exist.
        Directory.CreateDirectory(thumbnailsDir);
        var thumbnailPath = Path.Join(thumbnailsDir, meme.Id + "-" + memeFile.Name + ".png");

        var thumbnailFile = new FileInfo(thumbnailPath);
        // Check if the thumbnail already exists for whatever reason. This can sometimes happen if the DB is deleted and then recreated via an import.
        // Without this, FFmpeg will exit because the file it's trying to create already exists.
        if (thumbnailFile.Exists)
        {
            thumbnailFile.Delete();
        }

        await Task.Run(async () =>
        {
            // TODO: Check what Windows does. It definitely doesn't take the first frame. It might take a frame at some percentage of the way through the video
            var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(meme.Path, thumbnailPath, TimeSpan.FromSeconds(0));
            await conversion.Start();
        });

        using var image = await Image.LoadAsync(thumbnailPath);
        var size = image.Size();
        if (size.Height > size.Width)
        {
            image.Mutate(x => x.Resize(0, 200));
        }
        else
        {
            image.Mutate(x => x.Resize(200, 0));
        }
        await image.SaveAsync(thumbnailPath);

        return thumbnailPath;
    }

    private Meme.FileMediaType ClassifyFile(FileInfo file)
    {
        var fileExt = file.Extension.Replace(".", "");
        var fileType = fileExt switch
        {
            _ when s_imageExtensions.Contains(fileExt) => Meme.FileMediaType.Image,
            _ when s_animatedImageExtensions.Contains(fileExt) => Meme.FileMediaType.Gif,
            _ when s_videoExtensions.Contains(fileExt) => Meme.FileMediaType.Video,
            _ when s_audioExtensions.Contains(fileExt) => Meme.FileMediaType.Audio,
            _ => Meme.FileMediaType.Other
        };
        if (fileType.Equals(Meme.FileMediaType.Other))
        {
            _log.LogWarning(
                "Couldn't classify file '{File}' as an image, GIF, video, or audio file. If you believe this was a mistake, please make an issue in the repo",
                file.FullName);
        }

        return fileType;
    }

    private Category? CreateCategoryIfNotExists(string memesBaseDir, string memePath)
    {
        var relativePath = Path.GetRelativePath(memesBaseDir, memePath);
        if (!relativePath.Contains(Path.DirectorySeparatorChar))
        {
            return null;
        }

        // Omit the last entry as it will contain the file's name. We don't want that part of the path.
        var nestedCategories = relativePath.Split(Path.DirectorySeparatorChar).SkipLast(1);
        Category? currentParent = null;
        foreach (var categoryName in nestedCategories)
        {
            // Top level category
            if (currentParent == null)
            {
                var topLevel = _categoryService.GetTopLevelCategories();
                var existingTopLevelCategory = topLevel.FirstOrDefault(c => c.Name == categoryName);
                if (existingTopLevelCategory == null)
                {
                    var newTopLevelCategory = _categoryService.Create(new Category() { Name = categoryName });
                    currentParent = newTopLevelCategory;
                }
                else
                {
                    currentParent = existingTopLevelCategory;
                }

                continue;
            }
            else
            {
                // Check based on currentParent's children
                var existingCategory = currentParent.Children.FirstOrDefault(c => c.Name == categoryName);
                if (existingCategory == null)
                {
                    var newCategory =
                        _categoryService.Create(new Category() { Name = categoryName, Parent = currentParent });
                    currentParent = newCategory;
                }
                else
                {
                    currentParent = existingCategory;
                }
            }
        }

        if (currentParent != null)
        {
            using var context = new MemeManagerContext();
            // I can't get CategoryService.GetTopLevelCategories() to not fucking return tracked CategoryProxy objects
            context.Categories.Entry(currentParent).State = EntityState.Detached;
        }

        return currentParent;
    }
}
