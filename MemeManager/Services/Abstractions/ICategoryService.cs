using System.Collections;
using System.Collections.Generic;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface ICategoryService
{
    IEnumerable<Category> GetAll();

    IEnumerable<Category> GetTopLevelCategories();

    Category? GetById(int id);

    Category Create(Category newCategory);
}