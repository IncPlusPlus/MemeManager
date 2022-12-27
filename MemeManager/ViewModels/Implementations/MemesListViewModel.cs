using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using HanumanInstitute.MvvmDialogs;
using MemeManager.DependencyInjection;
using MemeManager.Models;
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
    private readonly ICategoryService _categoryService;
    private readonly IObservable<EventPattern<DbChangeEventArgs>> _dbChangedObservable;

    private readonly IDialogService _dialogService;

    // Here, we want to create a property to represent when the application 
    // is performing a search (i.e. when to show the "spinner" control that 
    // lets the user know that the app is busy). We also declare this property
    // to be the result of an Observable (i.e. its value is derived from 
    // some other property)
    private readonly ObservableAsPropertyHelper<bool> _isAvailable;
    private readonly IMemeService _memeService;

    private readonly ObservableAsPropertyHelper<IEnumerable<FileViewModel>> _searchResults;

    // In ReactiveUI, this is the syntax to declare a read-write property
    // that will notify Observers, as well as WPF/Avalonia, that a property has 
    // changed. If we declared this as a normal property, we couldn't tell when it has changed!
    private Category? _category;
    private IFilterObserverService _filterObserver;
    private ILogger _logger;
    private string? _searchString;

    public MemesListViewModel(ILogger logger, IDialogService dialogService,
        IFilterObserverService filterObserverService,
        IDbChangeNotifier dbChangeNotifierInstance, IMemeService memeService, ICategoryService categoryService)
    {
        _logger = logger;
        _dialogService = dialogService;
        _filterObserver = filterObserverService;
        _memeService = memeService;
        _categoryService = categoryService;
        SubscribeToEvents();

        ImplicitShowDialogCommand = ReactiveCommand.CreateFromTask<IList<object?>>(ImplicitShowDialog);

        /*
         * Create an Observable from the DbChanged event. There will be a new observation each time the event fires.
         * See https://www.reactiveui.net/docs/handbook/events/#how-do-i-convert-my-own-c-events-into-observables
         */
        _dbChangedObservable = Observable.FromEventPattern<EventHandler<DbChangeEventArgs>, DbChangeEventArgs>(
            handler => dbChangeNotifierInstance.EntitiesUpdated += handler,
            handler => dbChangeNotifierInstance.EntitiesUpdated -= handler);

        // https://www.reactiveui.net/docs/getting-started/compelling-example
        _searchResults = this
            // Might need to be WhenAny to allow for null values
            // Should be able to just add more properties like tags and keywords
            // Be sure to do .Select(term => term?.Trim()) for keywords
            .WhenAnyValue(x => x.CurrentCategory, x => x.CurrentSearchString)
            .Merge<(Category?, string?)>(this.WhenAnyObservable(x => x._dbChangedObservable)
                .Select(x => x.EventArgs)
                .Where(x => x.TypeRelevant(typeof(Meme)))
                .Select(x => (this.CurrentCategory, this.CurrentSearchString))
            )
            .Throttle(TimeSpan.FromMilliseconds(50))
            /*
             * Using DistinctUntilChanged causes the search results not to update on a category change because the
             * currently selected category and search string haven't changed. By merging the observable for the DbChanged event,
             * there will be a new observation but no new data. So, the current filters won't be distinct but we still want the search results to be updated.
             */
            // .DistinctUntilChanged()
            .SelectMany(SearchMemes)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.SearchResults);

        _searchResults.ThrownExceptions.Subscribe(error => _logger.LogError(error, "Error when searching for memes"));

        // A helper method we can use for Visibility or Spinners to show if results are available.
        // We get the latest value of the SearchResults and make sure it's not null.
        // TODO: This isn't used yet. Maybe show an indeterminate progress bar.
        _isAvailable = this
            .WhenAnyValue(x => x.SearchResults)
            .Select(searchResults => searchResults != null)
            .ToProperty(this, x => x.IsAvailable);

        SetCategoryCommand = ReactiveCommand.Create<ChangeCategorySelection>(SetCategory);
    }

    public ReactiveCommand<IList<object?>, Unit> ImplicitShowDialogCommand { get; }

    public List<CategoryTreeNodeModel> Categories =>
        _categoryService.GetTopLevelCategories().Select(x => new CategoryTreeNodeModel(x)).ToList();

    public ReactiveCommand<ChangeCategorySelection, Unit> SetCategoryCommand { get; }

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
    public object Selection { get; set; }

    private Task ImplicitShowDialog(IList<object?> memes) =>
        // The ownerViewModel is required to be a be the DataContext of a Window. I can't use 'this' like in the examples because MemesListViewModel is the DataContext of MemesListView which isn't a Window.
        ShowDialogAsync(
            viewModel => _dialogService.ShowDialogAsync<ChangeTagsCustomDialog>(
                Locator.Current.GetRequiredService<IMainWindowViewModel>() as MainWindowViewModel, viewModel), memes);

    public void SetCategory(ChangeCategorySelection path)
    {
        path.SelectedMemes.ForEach(m => _memeService.SetCategory(m, path.SelectedCategory));
    }

    // https://www.reactiveui.net/docs/getting-started/compelling-example
    private async Task<IEnumerable<FileViewModel>> SearchMemes((Category? category, string? searchString) searchTerms,
        CancellationToken token)
    {
        /*
         * TODO: This could be optimized to not have a .ToList call followed by another iteration by returning the
         * IQueryable<Meme> instance instead of the List
         */
        var filteredResults = await _memeService.GetFilteredAsync(searchTerms.category, searchTerms.searchString, token)
            .ConfigureAwait(false);
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

    private async Task ShowDialogAsync(Func<IChangeTagsCustomDialogViewModel, Task<bool?>> showDialogAsync,
        IList<object?> memes)
    {
        var dialogViewModel = _dialogService.CreateViewModel<IChangeTagsCustomDialogViewModel>();
        var selectedMemes = ((AvaloniaList<object>?)memes)?.Select(x => ((FileViewModel)x).Meme) ?? new List<Meme>();
        dialogViewModel.TargetMemes = selectedMemes;
        var success = await showDialogAsync(dialogViewModel).ConfigureAwait(true);
        Console.WriteLine();
        // if (success == true)
        // {
        //     Texts.Add(dialogViewModel.Text);
        // }
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

public class ChangeCategorySelection
{
    public IEnumerable<Meme> SelectedMemes { get; set; } = new List<Meme>();
    public Category? SelectedCategory { get; set; }
}

public class ChangeCategorySelectorConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        return new ChangeCategorySelection()
        {
            SelectedMemes =
                ((AvaloniaList<object>?)values[0])?.Select(x => ((FileViewModel)x).Meme) ?? new List<Meme>(),
            SelectedCategory = ((CategoryTreeNodeModel?)values[1])?.Category,
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
