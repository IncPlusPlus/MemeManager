using System;
using System.IO;
using System.Linq;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace MemeManager.Services.Implementations;

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

    public ImportService(ILogger logger, IDbChangeNotifier dbChangeNotifier, IMemeService memeService,
        ICategoryService categoryService, ITagService tagService)
    {
        _log = logger;
        _dbChangeNotifier = dbChangeNotifier;
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
     * - File extensions to include and what media type they are
     */
    public void ImportFromDirectory(string path)
    {
        try
        {
            _log.LogInformation("IMPORT OF MEMES FROM PATH {MemesPath} STARTING!", path);
            var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                // Skip memes we already know about
                if (_memeService.GetByPath(file) != null)
                    continue;
                var fileInfo = new FileInfo(file);
                var memeType = ClassifyFile(fileInfo);
                var memeCategory = CreateCategoryIfNotExists(path, file);
                // TODO: Maybe add a method to MemeService that allows for adding a list of Meme entities to the DB to avoid frequent DB writes and UI updates (because of the notifier being fired for every new meme)
                _memeService.Create(new Meme()
                {
                    Path = file,
                    Name = fileInfo.Name,
                    MediaType = memeType,
                    Category = memeCategory,
                    TimeAdded = DateTime.Now,
                    AdditionalTerms = ""
                });
            }

            _log.LogInformation("IMPORT OF MEMES FROM PATH {MemesPath} SUCCEEDED!", path);
        }
        catch (Exception e)
        {
            _log.LogError(e, "IMPORT OF MEMES FROM PATH {MemesPath} FAILED!", path);
        }
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

        return currentParent;
    }
}
