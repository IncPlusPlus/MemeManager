using System;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IFilterObserverService
{
    Category? CurrentCategory { get; set; }
    string? CurrentSearchTerms { get; set; }

    event EventHandler<EventArgs> CurrentCategoryChanged;
    event EventHandler<EventArgs> CurrentSearchTermsChanged;
}
