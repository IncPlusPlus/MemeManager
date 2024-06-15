using System.Collections.Generic;
using MongoDB.Bson;
using Realms;

namespace MemeManager.Persistence.Entity;

public partial class Tag : IRealmObject
{
    [PrimaryKey]
    public ObjectId Id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; }
    public IList<Meme> Memes { get; } = null!;

    public override string ToString()
    {
        return Name;
    }
}
