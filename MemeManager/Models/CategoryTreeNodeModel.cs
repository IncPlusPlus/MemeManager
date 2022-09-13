using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using MemeManager.Persistence.Entity;
using ReactiveUI;

namespace MemeManager.Models;

public class CategoryTreeNodeModel : ReactiveObject
{
    private Category _category;
    private string _name;
    private ObservableCollection<CategoryTreeNodeModel>? _children;
    private bool _hasChildren;
    private bool _isExpanded;
    private int _memeCount;

    public CategoryTreeNodeModel(Category category)
    {
        _category = category;
        _name = category.Name;
        _isExpanded = false;
        _hasChildren = category.Children?.Count > 0;
        _memeCount = category.Memes.Count;

        Category.Memes.CollectionChanged += MemesChanged;
        // TODO: Will likely have to subscribe to child category changes to reflect them in the UI
    }


    private void MemesChanged(object? sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
    {
        if (sender == null)
        {
            return;
        }
        var collection = (ObservableCollection<Meme>)sender;
        /*
         * For some reason, notifyCollectionChangedEventArgs.NewItems is always yielding null. I noticed that the sender
         * is an instance of the ObservableCollection itself. Because that object has the updated collection, I can get
         * the Count from there instead of from notifyCollectionChangedEventArgs.NewItems.
         */
        MemeCount = collection.Count;
    }

    public bool IsRoot => Category.Parent == null;

    public Category Category
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public string Name
    {
        get => _name;
        private set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public int MemeCount
    {
        get => _memeCount;
        private set => this.RaiseAndSetIfChanged(ref _memeCount, value);
    }

    public bool HasChildren
    {
        get => _hasChildren;
        private set => this.RaiseAndSetIfChanged(ref _hasChildren, value);
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        set => this.RaiseAndSetIfChanged(ref _isExpanded, value);
    }

    public IReadOnlyList<CategoryTreeNodeModel> Children => _children ??= LoadChildren();

    private ObservableCollection<CategoryTreeNodeModel> LoadChildren()
    {
        var options = new EnumerationOptions { IgnoreInaccessible = true };
        var result = new ObservableCollection<CategoryTreeNodeModel>();

        Category.Children?.ForEach(childCategory => result.Add(new CategoryTreeNodeModel(childCategory)));

        if (result.Count == 0)
            HasChildren = false;

        return result;
    }

    public static Comparison<CategoryTreeNodeModel?> SortAscending<T>(Func<CategoryTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return -1;
            else if (y is null)
                return 1;
            else
                return 1;
        };
    }

    public static Comparison<CategoryTreeNodeModel?> SortDescending<T>(Func<CategoryTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return 1;
            else if (y is null)
                return -1;
            else
                return 1;
        };
    }
}

public static class EnumerableExtensions
{
    /// <summary>
    /// One would hope that C#'s foreach loops would just not iterate if the collection is null but no. So now I need
    /// this stupid thing. Thanks to https://stackoverflow.com/a/6535813/1687436. 
    /// </summary>
    /// <param name="self"></param>
    /// <param name="action"></param>
    /// <typeparam name="T"></typeparam>
    public static void ForEach<T>(this IEnumerable<T>? self, Action<T> action)
    {
        if (self != null)
        {
            foreach (var element in self)
            {
                action(element);
            }
        }
    }
}
