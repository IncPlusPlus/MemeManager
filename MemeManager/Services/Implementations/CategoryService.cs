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
    private readonly ILogger _log;

    public CategoryService(MemeManagerContext context, ILogger logger)
    {
        _context = context;
        _log = logger;
    }


    public IEnumerable<Category> GetAll()
    {
        return _context.Categories.AsNoTracking().ToList();
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
        return newCategory;
    }
}
