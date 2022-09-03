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
    private readonly MemeManagerContext _context;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;

    public MemeService(MemeManagerContext context, IDbChangeNotifier dbChangeNotifier, ILogger logger)
    {
        _context = context;
        _dbChangeNotifier = dbChangeNotifier;
        _log = logger;
    }

    public IEnumerable<Meme> GetAll()
    {
        return _context.Memes.AsNoTracking().ToList();
    }

    public IEnumerable<Meme> GetFiltered(Category? category, string? searchTerms)
    {
        return GetFilteredInternal(category, searchTerms).ToList();
    }

    public Task<List<Meme>> GetFilteredAsync(Category? category, string? searchTerms, CancellationToken token)
    {
        return GetFilteredInternal(category, searchTerms).ToListAsync(token);
    }

    public Meme? GetById(int id)
    {
        return _context.Memes.AsNoTracking().SingleOrDefault(m => m.Id == id);
    }

    public Meme Create(Meme newMeme)
    {
        _context.Memes.Add(newMeme);
        _context.SaveChanges();
        return newMeme;
    }

    public Meme? DeleteById(int id)
    {
        var existingMeme = _context.Memes.SingleOrDefault(m => m.Id == id);
        try
        {
            return existingMeme != null ? _context.Memes.Remove(existingMeme).Entity : null;
        }
        finally
        {
            _context.SaveChanges();
        }
    }

    public Meme SetCategory(Meme meme, Category? category)
    {
        meme.Category = category;
        //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
        _context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Meme), typeof(Category) });
        return meme;
    }

    private IQueryable<Meme> GetFilteredInternal(Category? category, string? searchTerms)
    {
        _log.LogDebug("Starting search for category {CategoryName}...", category?.Name);
        var query = _context.Memes
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
