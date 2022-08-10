using System.Collections.Generic;
using System.Linq;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MemeManager.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly MemeManagerContext _context;

    public CategoryService(MemeManagerContext context)
    {
        _context = context;
    }


    public IEnumerable<Category> GetAll()
    {
        return _context.Categories.AsNoTracking().ToList();
    }

    public IEnumerable<Category> GetTopLevelCategories()
    {
        return _context.Categories.Include(c => c.Children).Where(category => category.Parent == null).ToList();
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