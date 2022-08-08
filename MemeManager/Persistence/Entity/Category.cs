using System.Collections.Generic;

namespace MemeManager.Persistence.Entity;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Path { get; set; }
    public virtual Category? Parent { get; set; }
    public virtual ICollection<Category> Children { get; set; }
    public virtual ICollection<Meme> Memes { get; set; }
    
    // TODO: Add a toString method that outputs ParentCategory/SubCategory/ThisCategory instead of just ThisCategory
}