using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using MemeManager.DependencyInjection;
using MemeManager.Persistence.Entity;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.ViewModels.Implementations;

public class MemesListViewModel : ViewModelBase, IMemesListViewModel
{
    // Here, we want to create a property to represent when the application 
    // is performing a search (i.e. when to show the "spinner" control that 
    // lets the user know that the app is busy). We also declare this property
    // to be the result of an Observable (i.e. its value is derived from 
    // some other property)
    private readonly ObservableAsPropertyHelper<bool> _isAvailable;

    private readonly ObservableAsPropertyHelper<IEnumerable<FileViewModel>> _searchResults;

    // In ReactiveUI, this is the syntax to declare a read-write property
    // that will notify Observers, as well as WPF/Avalonia, that a property has 
    // changed. If we declared this as a normal property, we couldn't tell when it has changed!
    private Category? _category;
    private string? _searchString;
    private IFilterObserverService _filterObserver;
    private ILogger _logger;
    private IMemeService _memeService;

    public MemesListViewModel(IFilterObserverService filterObserverService, IMemeService memeService)
    {
        _logger = Locator.Current.GetRequiredService<ILogger>();
        _filterObserver = filterObserverService;
        _memeService = memeService;
        SubscribeToEvents();

        // https://www.reactiveui.net/docs/getting-started/compelling-example
        _searchResults = this
            // Might need to be WhenAny to allow for null values
            // Should be able to just add more properties like tags and keywords
            // Be sure to do .Select(term => term?.Trim()) for keywords
            .WhenAnyValue(x => x.CurrentCategory, x => x.CurrentSearchString)
            .Throttle(TimeSpan.FromMilliseconds(50))
            .DistinctUntilChanged()
            .SelectMany(SearchMemes)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);

        _searchResults.ThrownExceptions.Subscribe(error => _logger.LogError(error, "Error when searching for memes"));

        // A helper method we can use for Visibility or Spinners to show if results are available.
        // We get the latest value of the SearchResults and make sure it's not null.
        _isAvailable = this
            .WhenAnyValue(x => x.SearchResults)
            .Select(searchResults => searchResults != null)
            .ToProperty(this, x => x.IsAvailable);
    }

    public Category? CurrentCategory
    {
        get => _category;
        set => this.RaiseAndSetIfChanged(ref _category, value);
    }

    public string? CurrentSearchString
    {
        get => _searchString;
        set => this.RaiseAndSetIfChanged(ref _searchString, value);
    }

    public IEnumerable<FileViewModel> SearchResults => _searchResults.Value;
    public bool IsAvailable => _isAvailable.Value;

    // https://www.reactiveui.net/docs/getting-started/compelling-example
    private async Task<IEnumerable<FileViewModel>> SearchMemes((Category? category, string? searchString) searchTerms, CancellationToken token)
    {
        /*
         * TODO: This could be optimized to not have a .ToList call followed by another iteration by returning the
         * IQueryable<Meme> instance instead of the List
         */
        var filteredResults = await _memeService.GetFilteredAsync(searchTerms.category, searchTerms.searchString, token).ConfigureAwait(false);
        _logger.LogDebug("Finished search for category {CategoryName}", searchTerms.category?.Name);
        return filteredResults.Select(x => new FileViewModel(x));
    }

    private void SubscribeToEvents()
    {
        _filterObserver.CurrentCategoryChanged += (_, _) =>
        {
            _logger.LogDebug("Category changed to {CategoryName}", _filterObserver.CurrentCategory?.Name);
            CurrentCategory = _filterObserver.CurrentCategory;
        };

        _filterObserver.CurrentSearchTermsChanged += (_, _) =>
        {
            _logger.LogDebug("Search terms: {Terms}", _filterObserver.CurrentSearchTerms);
            CurrentSearchString = _filterObserver.CurrentSearchTerms;
        };
    }
}

public class ThumbnailPathToBitmapConverter : IValueConverter
{
    public static ThumbnailPathToBitmapConverter Instance = new ThumbnailPathToBitmapConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        if (value is not string rawUri || !targetType.IsAssignableFrom(typeof(Bitmap)))
            throw new NotSupportedException();
        FileStream fs;

        if (File.Exists(rawUri))
        {
            fs = File.OpenRead(rawUri);
        }
        else
        {
            throw new Exception("Thumbnail not found");
        }

        return new Bitmap(fs);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}