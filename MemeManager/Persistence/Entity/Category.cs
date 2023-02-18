using System.Collections.ObjectModel;

namespace MemeManager.Persistence.Entity;

public class Category
{
    private readonly ObservableCollection<Category> _children =
        new ObservableCollection<Category>();

    private readonly ObservableCollection<Meme> _memes =
        new ObservableCollection<Meme>();

    public int Id { get; set; }
    public string Name { get; set; }

    public virtual Category? Parent { get; set; }

    // https://docs.microsoft.com/en-us/ef/ef6/querying/local-data#wpf-binding-to-navigation-properties
    public virtual ObservableCollection<Category> Children { get { return _children; } }
    public virtual ObservableCollection<Meme> Memes { get { return _memes; } }

    // TODO: Add a toString method that outputs ParentCategory/SubCategory/ThisCategory instead of just ThisCategory
}
