using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Implementations;
using MemeManager.ViewModels.Interfaces;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.DependencyInjection;

public static class ViewModelsBootstrapper
{
    public static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterObservers(services, resolver);
        RegisterServices(services, resolver);
        RegisterFactories(services, resolver);
    }

    private static void RegisterObservers(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IFilterObserverService>(() => new FilterObserverService());
    }

    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var dbChangeNotifier = resolver.GetRequiredService<IDbChangeNotifier>();
        var filtersObserver = resolver.GetRequiredService<IFilterObserverService>();
        services.RegisterLazySingleton<ISearchbarViewModel>(() =>
            new SearchbarViewModel(filtersObserver));
        services.RegisterLazySingleton<ICategoriesListViewModel>(() =>
            new CategoriesListViewModel(filtersObserver, dbChangeNotifier, resolver.GetRequiredService<ICategoryService>()));
        services.RegisterLazySingleton<IMemesListViewModel>(() =>
            new MemesListViewModel(resolver.GetRequiredService<ILogger>(), filtersObserver, dbChangeNotifier, resolver.GetRequiredService<IMemeService>(), resolver.GetRequiredService<ICategoryService>()));
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ISearchbarViewModel>(),
            resolver.GetRequiredService<ICategoriesListViewModel>(),
            resolver.GetRequiredService<IMemesListViewModel>(),
            resolver.GetRequiredService<LayoutConfiguration>()
        ));
    }

    private static void RegisterFactories(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
    }
}
