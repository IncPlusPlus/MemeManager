using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace MemeManager.Persistence.Entity;

public partial class Category : IRealmObject
{
    public IList<Category> Children { get; } = null!;

    public IList<Meme> Memes { get; } = null!;

    [PrimaryKey]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; }

    public Category? Parent { get; set; }

    // https://docs.microsoft.com/en-us/ef/ef6/querying/local-data#wpf-binding-to-navigation-properties
    // public IList<Category> Children { get { return _children; } }
    // public IList<Meme> Memes { get { return _memes; } }

    // TODO: Add a toString method that outputs ParentCategory/SubCategory/ThisCategory instead of just ThisCategory
}
