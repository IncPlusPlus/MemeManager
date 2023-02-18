using System.Collections.Generic;
using MemeManager.Persistence;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface ITagService
{
    ITagService NewInstance(MemeManagerContext separateContext);
    IEnumerable<Tag> GetAll();

    Tag? GetById(int id);

    Tag Create(Tag newTag);

    Tag? DeleteById(int id);
}
