using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.DependencyInjection;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.Services.Implementations;

public class MemeService : IMemeService
{
    private readonly MemeManagerContext _context;

    public MemeService(MemeManagerContext context)
    {
        _context = context;
    }

    public IEnumerable<Meme> GetAll()
    {
        return _context.Memes.AsNoTracking().ToList();
    }

    public IEnumerable<Meme> GetFiltered(Category? category)
    {
        return GetFilteredInternal(category).ToList();
    }

    public Task<List<Meme>> GetFilteredAsync(Category? category, CancellationToken token)
    {
        return GetFilteredInternal(category).ToListAsync(token);
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
        return existingMeme != null ? _context.Memes.Remove(existingMeme).Entity : null;
    }

    private IQueryable<Meme> GetFilteredInternal(Category? category)
    {
        var logger = Locator.Current.GetRequiredService<ILogger>();
        logger.LogDebug("Starting search for category {CategoryName}...", category?.Name);
        return _context.Memes
            // Return all memes if the category is null. Otherwise, filter by the category.
            .Where(meme => category == null || meme.Category == category);
    }
}