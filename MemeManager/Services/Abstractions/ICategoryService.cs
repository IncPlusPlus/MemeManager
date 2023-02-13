using System.Collections.Generic;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface ICategoryService
{
    ICategoryService NewInstance(MemeManagerContext separateContext);
    IEnumerable<Category> GetAll();

    IEnumerable<Category> GetTopLevelCategories();

    Category? GetById(int id);

    Category Create(Category newCategory);

    void Delete(params Category[] categories);

    Category Rename(Category category, string name);
}
