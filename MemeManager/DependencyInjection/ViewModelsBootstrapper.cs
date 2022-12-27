using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Implementations;
using MemeManager.ViewModels.Interfaces;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.DependencyInjection;

public static class ViewModelsBootstrapper
{
    public static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterViewModelHelpers(services, resolver);
        RegisterObservers(services, resolver);
        RegisterServices(services, resolver);
        RegisterFactories(services, resolver);
    }

    private static void RegisterViewModelHelpers(IMutableDependencyResolver services,
        IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IDialogService>(() => new DialogService(
            new DialogManager(
                logger: resolver.GetRequiredService<ILoggerFactory>().CreateLogger<DialogManager>(),
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddFluent()),
            viewModelFactory: resolver.GetRequiredService));
    }

    private static void RegisterObservers(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IFilterObserverService>(() => new FilterObserverService());
    }

    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var dbChangeNotifier = resolver.GetRequiredService<IDbChangeNotifier>();
        var filtersObserver = resolver.GetRequiredService<IFilterObserverService>();

        var memeService = resolver.GetRequiredService<IMemeService>();
        var categoryService = resolver.GetRequiredService<ICategoryService>();
        var tagService = resolver.GetRequiredService<ITagService>();
        services.RegisterLazySingleton<ISearchbarViewModel>(() =>
            new SearchbarViewModel(filtersObserver));
        services.RegisterLazySingleton<ICategoriesListViewModel>(() =>
            new CategoriesListViewModel(filtersObserver, dbChangeNotifier, categoryService, memeService));
        services.RegisterLazySingleton<IMemesListViewModel>(() =>
            new MemesListViewModel(resolver.GetRequiredService<ILogger>(),
                resolver.GetRequiredService<IDialogService>(), filtersObserver, dbChangeNotifier, memeService,
                categoryService));
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ISearchbarViewModel>(),
            resolver.GetRequiredService<ICategoriesListViewModel>(),
            resolver.GetRequiredService<IMemesListViewModel>(),
            resolver.GetRequiredService<LayoutConfiguration>()
        ));
        services.RegisterLazySingleton<IChangeTagsCustomDialogViewModel>(() =>
            new ChangeTagsCustomDialogViewModel(memeService, tagService));
    }

    private static void RegisterFactories(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
    }
}
