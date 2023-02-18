using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.Extensions;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NinjaNye.SearchExtensions;

namespace MemeManager.Services.Implementations;

public class MemeService : IMemeService
{
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;

    public MemeService(MemeManagerContext context, IDbChangeNotifier dbChangeNotifier, ILogger logger)
    {
        _dbChangeNotifier = dbChangeNotifier;
        _log = logger;
    }

    public IMemeService NewInstance(MemeManagerContext separateContext)
    {
        return new MemeService(separateContext, this._dbChangeNotifier, this._log);
    }

    public IEnumerable<Meme> GetAll()
    {
        using var context = new MemeManagerContext();
        return context.Memes.AsNoTracking().ToList();
    }

    public IEnumerable<Meme> GetFiltered(Category? category, string? searchTerms)
    {
        using var context = new MemeManagerContext();
        return GetFilteredInternal(category, searchTerms, context).ToList();
    }

    public Task<List<Meme>> GetFilteredAsync(Category? category, string? searchTerms, CancellationToken token)
    {
        using var context = new MemeManagerContext();
        return GetFilteredInternal(category, searchTerms, context).ToListAsync(token);
    }

    public Meme? GetById(int id)
    {
        using var context = new MemeManagerContext();
        return context.Memes.AsNoTracking().SingleOrDefault(m => m.Id == id);
    }

    public Meme? GetByPath(string path)
    {
        using var context = new MemeManagerContext();
        return context.Memes.AsNoTracking().SingleOrDefault(m => m.Path == path);
    }

    public Meme Create(Meme newMeme)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(newMeme);
        context.Memes.Add(newMeme);
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme) });
        return newMeme;
    }

    public Meme? DeleteById(int id)
    {
        using var context = new MemeManagerContext();
        var existingMeme = context.Memes.SingleOrDefault(m => m.Id == id);
        try
        {
            return existingMeme != null ? context.Memes.Remove(existingMeme).Entity : null;
        }
        finally
        {
            context.SaveChanges();
        }
    }

    public Meme SetCategory(Meme meme, Category? category)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(meme);
        meme.Category = category;
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme), typeof(Category) });
        return meme;
    }

    public Meme SetTags(Meme meme, params Tag[] tags)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(meme);
        meme.Tags = tags;
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme), typeof(Tag) });
        return meme;
    }

    public Meme AddTag(Meme meme, Tag tag)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(meme);
        meme.Tags.Add(tag);
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme), typeof(Tag) });
        return meme;
    }

    public Meme RemoveTag(Meme meme, Tag tag)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(meme);
        meme.Tags.Remove(tag);
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme), typeof(Tag) });
        return meme;
    }

    public Meme SetThumbnailPath(Meme meme, string? thumbnailPath)
    {
        using var context = new MemeManagerContext();
        context.Memes.Attach(meme);
        meme.CachedThumbnailPath = thumbnailPath;
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        //TODO: The above TODO seems to not be true. Investigate if I still need to make any change pertaining to the above comment.
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme) });
        return meme;
    }

    public void SetThumbnailPaths(IEnumerable<(Meme, string?)> thumbnails)
    {
        foreach (var tuple in thumbnails)
        {
            using var context = new MemeManagerContext();
            var (meme, thumbnailPath) = tuple;
            context.Memes.Attach(meme);
            meme.CachedThumbnailPath = thumbnailPath;
            context.SaveChanges();
        }

        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme) });
    }

    private IQueryable<Meme> GetFilteredInternal(Category? category, string? searchTerms, MemeManagerContext context)
    {
        // TODO: Maybe add an option to include memes from all child categories as well
        _log.LogDebug("Starting search for category {CategoryName}...", category?.Name);
        var query = context.Memes
            // Return all memes if the category is null. Otherwise, filter by the category.
            .Where(meme => category == null || meme.Category == category);
        if (searchTerms != null && searchTerms.Length > 0)
        {
            query = query
                // Currently this is case sensitive https://github.com/ninjanye/SearchExtensions/issues/42
                .Search(
                    x => x.AdditionalTerms
                    // ,
                    // TODO: This seems to select all memes. It needs to select all tags that contain the search string.
                    // x => x.Tags.Select(t => t.Name).Aggregate((s1, s2) => $"{s1} {s2}")
                )
                .Containing(searchTerms?.Split(' '))
                // Call this function last to help with compatability issue https://github.com/ninjanye/SearchExtensions/issues/40
                .Apply();
        }

        return query;
    }
}
