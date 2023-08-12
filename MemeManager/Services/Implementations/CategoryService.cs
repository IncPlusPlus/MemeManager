using System.Collections.Generic;
using System.Linq;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MemeManager.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly MemeManagerContext _context;
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;

    public CategoryService(MemeManagerContext context, IDbChangeNotifier dbChangeNotifier, ILogger logger)
    {
        _context = context;
        _dbChangeNotifier = dbChangeNotifier;
        _log = logger;
    }


    public IEnumerable<Category> GetAll(bool asNoTracking = true)
    {
        return asNoTracking ? _context.Categories.AsNoTracking().ToList() : _context.Categories.ToList();
    }

    public IEnumerable<Category> GetTopLevelCategories()
    {
        return _context.Categories
            /*
             * Loading just the categories is a simple operation and we don't want to pepper the DB with queries
             * when we want to expand a bunch of categories so we explicitly/eagerly load the children of the
             * categories.
             * See https://docs.microsoft.com/en-us/ef/core/querying/related-data/ for more info
             */
            .Include(c => c.Children)
            .Where(category => category.Parent == null).ToList();
    }

    public Category? GetById(int id)
    {
        return _context.Categories.AsNoTracking().SingleOrDefault(c => c.Id == id);
    }

    public Category Create(Category newCategory)
    {
        _context.Categories.Add(newCategory);
        _context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
        return newCategory;
    }

    public void BulkCreate(IEnumerable<Category> categories)
    {
        _context.Categories.AddRange(categories);
        _context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
    }

    public void Delete(params Category[] categories)
    {
        // TODO: Figure out what will happen with memes that are in this category. Might need to unset their category manually.
        // TODO: Deal with orphaned memes. Maybe there needs to be a default category named "uncategorized"
        foreach (var category in categories)
        {
            // TODO: If category has any children, they need to be deleted too
            _context.Categories.Remove(category);
        }

        _context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
    }

    public Category Rename(Category category, string name)
    {
        category.Name = name;
        _context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
        return category;
    }
}
