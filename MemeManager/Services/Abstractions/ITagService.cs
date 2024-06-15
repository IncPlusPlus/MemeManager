using System.Collections.Generic;
using MemeManager.Persistence.Entity;
using MongoDB.Bson;

namespace MemeManager.Services.Abstractions;

public interface ITagService
{
    IEnumerable<Tag> GetAll();

    Tag? GetById(ObjectId id);

    Tag Create(Tag newTag);

    Tag? DeleteById(ObjectId id);
}
