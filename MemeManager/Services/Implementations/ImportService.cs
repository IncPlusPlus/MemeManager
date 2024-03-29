﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.Extensions;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace MemeManager.Services.Implementations;

public class ImportService : IImportService
{
    // TODO: If I ever get around to adding tests, I should add a test that makes sure that the union of these file extensions arrays is an empty array
    // https://developer.mozilla.org/en-US/docs/Web/Media/Formats/Image_types
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly string[] s_imageExtensions = new[]
    {
        "jpg", "jpeg", "jfif", "pjpeg", "pjp", "png", "webp", "tif", "tiff", "bmp", "heic", "heif", "avif"
    };

    [SuppressMessage("ReSharper", "StringLiteralTypo")] private static readonly string[] s_animatedImageExtensions = new[] { "gif", "apng" };

    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    private static readonly string[] s_videoExtensions = new[]
    {
        "mp4", "avi", "mov", "wmv", "mkv", "webm", "flv", "f4v", "swf", "avchd", "ass", "ts", "3gp"
    };

    [SuppressMessage("ReSharper", "StringLiteralTypo")] private static readonly string[] s_audioExtensions = new[] { "wav", "flac", "ogg", "mp3", "alac", "aac" };
    private readonly ICategoryService _categoryService;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly IImportRequestNotifier _importRequestNotifier;
    private readonly ILogger _log;
    private readonly IMemeService _memeService;
    private readonly IStatusService _statusService;
    private readonly ITagService _tagService;

    public ImportService(ILogger logger, IDbChangeNotifier dbChangeNotifier,
        IImportRequestNotifier importRequestNotifier, IStatusService statusService, IMemeService memeService,
        ICategoryService categoryService, ITagService tagService)
    {
        _log = logger;
        _dbChangeNotifier = dbChangeNotifier;
        _importRequestNotifier = importRequestNotifier;
        _statusService = statusService;
        _memeService = memeService;
        _categoryService = categoryService;
        _tagService = tagService;
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
    public void ImportFromDirectory(string path)
    {
        _log.LogInformation("Import requested for path: {MemesPath}", path);
        try
        {
            _importRequestNotifier.SendImportRequest(path);
        }
        catch (Exception e)
        {
            _log.LogError(e, "Failed to send import request for memes on path {MemesPath}", path);
        }
    }

    // TODO: Break this up so that the creation of categories is:
    // 1. Cached such that instead of checking the database, a cache can be checked to see if a category for a certain path exists
    // 2. Uses it's own category creation method that
    //      1. Doesn't notify the UI of a new category being created
    //      2. Uses its own DBContext instance so as to not run into any DBContext multi-thread access issues if the stupid idiot dumdum retard user decides to perform any action that requires DB access
    public void ImportFromPaths(string basePath)
    {
        _log.LogInformation("Scanning path: {MemesPath}", basePath);
        var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
        _log.LogInformation("Starting import of {NumMemes} files from path {MemesPath}", files.Length, basePath);

        var cats = CreateCategoriesForPaths(basePath, files);
        var startTimeIteration = Stopwatch.GetTimestamp();
        var importJobNum = _statusService.AddJob("Importing memes", 0, files.Length);
        try
        {
            var memesToImport = new List<Meme>(files.Length);
            foreach (var (file, index) in files.WithIndex())
            {
                // Skip memes we already know about
                if (_memeService.GetByPath(file) != null)
                {
                    _statusService.UpdateJob(importJobNum, index, files.Length);
                    continue;
                }
                var fileInfo = new FileInfo(file);
                var memeType = ClassifyFile(fileInfo);
                var categoryString = string.Join(Path.DirectorySeparatorChar,
                    Path.GetRelativePath(basePath, file).Split(Path.DirectorySeparatorChar).SkipLast(1));
                var memeCategory = categoryString.Length > 0 ? cats[categoryString] : null;
                memesToImport.Add(new Meme()
                {
                    Path = file,
                    Name = fileInfo.Name,
                    MediaType = memeType,
                    Category = memeCategory,
                    TimeAdded = DateTime.Now,
                    AdditionalTerms = ""
                });
                _statusService.UpdateJob(importJobNum, index, files.Length);
            }
            var elapsedTimeIteration = Stopwatch.GetElapsedTime(startTimeIteration);
            _log.LogInformation("Finished iterating and creating categories in {Time}", elapsedTimeIteration);
            _log.LogTrace("Created all necessary categories. Attempting to save new memes to the database...");
            var startTimeSave = Stopwatch.GetTimestamp();
            _memeService.BulkCreate(memesToImport);
            var elapsedTimeSave = Stopwatch.GetElapsedTime(startTimeSave);
            _log.LogInformation("Saved {NumMemes} to the DB in {Time}", files.Length, elapsedTimeSave);
            _log.LogInformation("Import of memes from path {MemesPath} succeeded!", basePath);
            _log.LogTrace("Sending GenerateThumbnailsRequest");
            _importRequestNotifier.SendGenerateThumbnailsRequest(memesToImport);
            _log.LogTrace("GenerateThumbnailsRequest sent");
        }
        catch (Exception e)
        {
            _log.LogError(e, "IMPORT OF MEMES FROM PATH {MemesPath} FAILED!", basePath);
        }
        finally
        {
            _statusService.RemoveJob(importJobNum);
        }
    }

    public async Task GenerateThumbnails(IEnumerable<Meme> memes)
    {
        _log.LogInformation("Starting thumbnail generation");
        if (!memes.TryGetNonEnumeratedCount(out var count))
        {
            _log.LogError("Tried to count the number of memes to generate thumbnails for but failed");
        }

        var thumbnailJobId = _statusService.AddJob("Generating thumbnails", 0, count);
        var startTime = Stopwatch.GetTimestamp();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };
        var current = 0;
        var memesAndThumbnailPaths = new ConcurrentBag<(Meme, string?)>();

        // TODO: If I ever add cancellable tasks, this cancellation token will come in handy
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
                memesAndThumbnailPaths.Add((meme, thumbnailPath));
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to generate thumbnail for meme at path '{MemePath}'", meme.Path);
            }
            finally
            {
                Interlocked.Increment(ref current);
                _statusService.UpdateJob(thumbnailJobId, current, count);
            }
        });

        var elapsedTime = Stopwatch.GetElapsedTime(startTime);
        _log.LogInformation("Finished generating thumbnails in {Time}", elapsedTime);
        _statusService.RemoveJob(thumbnailJobId);
        _importRequestNotifier.SendSetThumbnailsRequest(memesAndThumbnailPaths);
    }

    public void SetThumbnails(IEnumerable<(Meme, string?)> memesAndThumbnailPaths)
    {
        _memeService.SetThumbnailPaths(memesAndThumbnailPaths);
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
        // var firstFrame = image.Frames[0];
        var firstFrame = frames.CloneFrame(0);
        // GifFrameMetadata metaData = firstFrame.Metadata.GetGifMetadata();
        // var frameDelay = metaData.FrameDelay;
        // firstFrame = image.Frames[metaData.FrameDelay];
        // var result = new Image<Rgba32>(size.Width, size.Height);
        // result[0, 0] = firstFrame.;
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


        // foreach (ImageFrame frame in image.Frames)
        // {
        //     GifFrameMetadata metaData = frame.Metadata.GetGifMetadata();
        //     // metaData.FrameDelay = frameDelay;
        //     // metaData.ColorTableLength = colorTableLength;
        //     // metaData.DisposalMethod = disposalMethod;
        // }
        //
        // image.Mutate(x => x.Resize(0,200));
        // await image.SaveAsync(thumbnailPath);

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
            var conversion =
                await FFmpeg.Conversions.FromSnippet.Snapshot(meme.Path, thumbnailPath, TimeSpan.FromSeconds(0));
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

    private Dictionary<string, Category?> CreateCategoriesForPaths(string memesBaseDir, string[] files)
    {
        var startTimeCats = Stopwatch.GetTimestamp();
        // Requires GetAll to NOT have asNoTracking applied to it because we need the Parent property to be loaded
        Dictionary<string, Category?> existingCategories = _categoryService.GetAll(asNoTracking: false).ToDictionary(
            category =>
            {
                var categoryPathString = category.Name;
                var currentCategory = category;
                while (currentCategory != null)
                {
                    categoryPathString =
                        (currentCategory.Parent == null ?
                            "" :
                            currentCategory.Parent.Name + Path.DirectorySeparatorChar) + categoryPathString;
                    currentCategory = currentCategory.Parent;
                }

                return categoryPathString;
            })!;
        var existingAndNewCategories = files.Select(s => Path.GetRelativePath(memesBaseDir, s))
            .Where(s => s.Contains(Path.DirectorySeparatorChar))
            .Select(s => string.Join(Path.DirectorySeparatorChar, s.Split(Path.DirectorySeparatorChar).SkipLast(1)))
            .Distinct().ToDictionary(s => s,
                s => existingCategories.TryGetValue(s, out var existingCat) ? existingCat : null);

        var merged = existingCategories
            .Concat(existingAndNewCategories.Where(kvp => !existingCategories.ContainsKey(kvp.Key)))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var categoryJobNum = _statusService.AddJob("Creating categories", 0, existingAndNewCategories.Count);
        var categoriesToCreate = new List<Category>();
        foreach (var (entry, index) in existingAndNewCategories.WithIndex())
        {
            if (entry.Value == null)
            {
                var possiblyNewCat = CreateCategoryIfNotExists(entry.Key, merged, 0);
                if (possiblyNewCat != null)
                {
                    categoriesToCreate.Add(possiblyNewCat);
                }
            }
            _statusService.UpdateJob(categoryJobNum, index, existingAndNewCategories.Count);
        }


        var elapsedTimeCats = Stopwatch.GetElapsedTime(startTimeCats);
        _log.LogInformation("Finished creating all categories in {Time}", elapsedTimeCats);
        _categoryService.BulkCreate(categoriesToCreate);
        _statusService.RemoveJob(categoryJobNum);

        return merged;
    }

    private Category? CreateCategoryIfNotExists(string catPath, Dictionary<string, Category?> cats, int depth)
    {
        var catPathParts = catPath.Split(Path.DirectorySeparatorChar).ToArray();
        if (depth == catPathParts.Length)
        {
            // Not even the first part of the category path exists as a category. Create a category for each part of the path.
            var deepestCat = new Category() { Name = catPathParts[^1] };
            cats[string.Join(Path.DirectorySeparatorChar, catPathParts)] = deepestCat;
            var currCat = deepestCat;
            for (var i = 1; i < catPathParts.Length; i++)
            {
                currCat.Parent = new Category() { Name = catPathParts[^(1 + i)] };
                cats[string.Join(Path.DirectorySeparatorChar, catPathParts.SkipLast(i))] = currCat.Parent;
                currCat = currCat.Parent;
            }

            return deepestCat;
        }
        if (cats.TryGetValue(string.Join(Path.DirectorySeparatorChar, catPathParts.SkipLast(depth)), out var existingCat) && existingCat != null)
        {
            // Found base
            var deepestCat = new Category() { Name = catPathParts[^1] };
            cats[string.Join(Path.DirectorySeparatorChar, catPathParts)] = deepestCat;
            var currCat = deepestCat;

            for (int i = 1; i < depth; i++)
            {
                currCat.Parent = new Category() { Name = catPathParts[^(1 + i)] };
                cats[string.Join(Path.DirectorySeparatorChar, catPathParts.SkipLast(i))] = currCat.Parent;
                currCat = currCat.Parent;
            }
            currCat.Parent = existingCat;
            return deepestCat;
        }
        else
        {
            return CreateCategoryIfNotExists(catPath, cats, depth + 1);
        }
    }
}
