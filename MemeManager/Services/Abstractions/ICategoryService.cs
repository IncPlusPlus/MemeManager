using System.Collections.Generic;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface ICategoryService
{
    IEnumerable<Category> GetAll(bool asNoTracking);

    IEnumerable<Category> GetTopLevelCategories();

    Category? GetById(int id);

    Category Create(Category newCategory);
    void BulkCreate(IEnumerable<Category> newCategory);

    void Delete(params Category[] categories);

    Category Rename(Category category, string name);
}
