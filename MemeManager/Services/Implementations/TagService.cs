using System.Collections.Generic;
using System.Linq;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MemeManager.Services.Implementations;

public class TagService : ITagService
{
    private readonly MemeManagerContext _context;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;

    public TagService(MemeManagerContext context, IDbChangeNotifier dbChangeNotifier, ILogger logger)
    {
        _context = context;
        _dbChangeNotifier = dbChangeNotifier;
        _log = logger;
    }

    public IEnumerable<Tag> GetAll()
    {
        return _context.Tags.ToList();
    }

    public Tag? GetById(int id)
    {
        return _context.Tags.AsNoTracking().SingleOrDefault(t => t.Id == id);
    }

    public Tag Create(Tag newTag)
    {
        _context.Tags.Add(newTag);
        _context.SaveChanges();
        return newTag;
    }

    public Tag? DeleteById(int id)
    {
        // TODO: Double check that Memes that use this tag have this tag removed from them
        var existingTag = _context.Tags.SingleOrDefault(m => m.Id == id);
        try
        {
            return existingTag != null ? _context.Tags.Remove(existingTag).Entity : null;
        }
        finally
        {
            //TODO: The viewmodels don't reflect the changed data until a query is run again. Maybe fire an event here again or do the ReactiveUI this.WhenAnyValue() stuff
            _context.SaveChanges();
            _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Tag) });
        }
    }
}
