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
    private readonly IDbChangeNotifier _dbChangeNotifier;
    private readonly ILogger _log;

    public CategoryService(MemeManagerContext context, IDbChangeNotifier dbChangeNotifier, ILogger logger)
    {
        _dbChangeNotifier = dbChangeNotifier;
        _log = logger;
    }


    public ICategoryService NewInstance(MemeManagerContext separateContext)
    {
        return new CategoryService(separateContext, _dbChangeNotifier, _log);
    }

    public IEnumerable<Category> GetAll()
    {
        using var context = new MemeManagerContext();
        return context.Categories.AsNoTracking().ToList();
    }

    public IEnumerable<Category> GetTopLevelCategories()
    {
        using var context = new MemeManagerContext();
        return context.Categories
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
        using var context = new MemeManagerContext();
        return context.Categories.AsNoTracking().SingleOrDefault(c => c.Id == id);
    }

    public Category Create(Category newCategory)
    {
        using var context = new MemeManagerContext();
        context.Categories.Attach(newCategory);
        context.Categories.Add(newCategory);
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
        // context.Categories.Entry(newCategory).State = EntityState.Detached;
        return newCategory;
    }

    public void Delete(params Category[] categories)
    {
        using var context = new MemeManagerContext();
        // TODO: Figure out what will happen with memes that are in this category. Might need to unset their category manually.
        // TODO: Deal with orphaned memes. Maybe there needs to be a default category named "uncategorized"
        foreach (var category in categories)
        {
            // TODO: If category has any children, they need to be deleted too
            context.Categories.Remove(category);
        }

        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
    }

    public Category Rename(Category category, string name)
    {
        using var context = new MemeManagerContext();
        context.Categories.Attach(category);
        category.Name = name;
        context.SaveChanges();
        _dbChangeNotifier.NotifyOfChanges(new[] { typeof(Category) });
        return category;
    }
}
