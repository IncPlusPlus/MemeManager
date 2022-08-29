using System;
using System.Reactive.Linq;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class SearchbarViewModel : ReactiveObject, ISearchbarViewModel
{
    private IFilterObserverService _filterObserver;
    private string? _searchText;

    public SearchbarViewModel(IFilterObserverService filterObserverService)
    {
        _filterObserver = filterObserverService;

        this.WhenAnyValue(x => x.SearchText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(NotifyObserver!);
    }

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    private async void NotifyObserver(string s)
    {
        _filterObserver.CurrentSearchTerms = s;
    }
}