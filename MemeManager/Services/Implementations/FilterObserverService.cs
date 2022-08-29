using System;
using MemeManager.Extensions;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;

namespace MemeManager.Services.Implementations;

public class FilterObserverService : IFilterObserverService
{
    private Category? _category;
    private string? _searchTerms;

    public Category? CurrentCategory
    {
        get => _category;
        set
        {
            _category = value;
            CurrentCategoryChanged.Raise(this, EventArgs.Empty);
        }
    }

    public string? CurrentSearchTerms
    {
        get => _searchTerms;
        set
        {
            _searchTerms = value;
            CurrentSearchTermsChanged.Raise(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs>? CurrentCategoryChanged;
    public event EventHandler<EventArgs>? CurrentSearchTermsChanged;

    // TODO: Add the search stuff to this class too. Look into the avalonia musicstore example. That uses LINQ's throttle method which might be useful here or some equivalent event raising method to throttle them.
}