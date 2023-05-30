using System;
using System.Reactive;
using System.Reactive.Linq;
using MemeManager.Services.Abstractions;
using MemeManager.ViewModels.Interfaces;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace MemeManager.ViewModels.Implementations;

public class StatusBarViewModel : ReactiveObject, IStatusBarViewModel
{
    private readonly ObservableAsPropertyHelper<int> _currentProgress;
    private readonly ObservableAsPropertyHelper<bool> _hasProgress;
    private readonly ObservableAsPropertyHelper<bool> _hasRunningJobs;
    private readonly ILogger _log;
    private readonly ObservableAsPropertyHelper<int> _maximumProgress;
    private readonly IObservable<EventPattern<StatusChangedArgs>> _statusChangedObservable;
    private readonly IStatusService _statusService;
    private readonly ObservableAsPropertyHelper<string> _statusText;

    public StatusBarViewModel(ILogger logger, IStatusService statusService)
    {
        _log = logger;
        _statusService = statusService;

        /*
         * Create an Observable from the StatusChanged event. There will be a new observation each time the event fires.
         * See https://www.reactiveui.net/docs/handbook/events/#how-do-i-convert-my-own-c-events-into-observables
         */
        _statusChangedObservable = Observable.FromEventPattern<EventHandler<StatusChangedArgs>, StatusChangedArgs>(
            handler => _statusService.StatusChanged += handler,
            handler => _statusService.StatusChanged -= handler);

        _statusText = this.WhenAnyObservable(x => x._statusChangedObservable)
            .Select(x => x.EventArgs.StatusText)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.StatusText);

        _currentProgress = this.WhenAnyObservable(x => x._statusChangedObservable)
            .Select(x => x.EventArgs.TotalCurrentProgress)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.CurrentProgress);

        _maximumProgress = this.WhenAnyObservable(x => x._statusChangedObservable)
            .Select(x => x.EventArgs.TotalMaxProgress)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.MaximumProgress);

        _hasProgress = this.WhenAnyObservable(x => x._statusChangedObservable)
            .Select(x => x.EventArgs.HasProgress)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.HasProgress);

        _hasRunningJobs = this.WhenAnyObservable(x => x._statusChangedObservable)
            .Select(x => x.EventArgs.NumJobs)
            .Select(x => x > 0)
            .ObserveOn(RxApp.MainThreadScheduler)
            .ToProperty(this, x => x.HasRunningJobs);
    }

    public string StatusText => _statusText.Value;

    public int MaximumProgress => _maximumProgress.Value;

    public int CurrentProgress => _currentProgress.Value;

    public bool HasProgress => _hasProgress.Value;
    public bool HasRunningJobs => _hasRunningJobs.Value;
}
