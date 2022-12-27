using System.Collections.Generic;

namespace MemeManager.Persistence.Entity;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Meme> Memes { get; set; }

    public override string ToString()
    {
        return Name;
    }
}
