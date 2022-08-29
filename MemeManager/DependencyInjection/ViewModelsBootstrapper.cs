using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using MemeManager.ViewModels.Configuration;
using MemeManager.ViewModels.Implementations;
using MemeManager.ViewModels.Interfaces;
using Splat;

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
        var filtersObserver = resolver.GetRequiredService<IFilterObserverService>();
        services.RegisterLazySingleton<ISearchbarViewModel>(() =>
            new SearchbarViewModel(filtersObserver));
        services.RegisterLazySingleton<ICategoriesListViewModel>(() =>
            new CategoriesListViewModel(filtersObserver, resolver.GetRequiredService<ICategoryService>()));
        services.RegisterLazySingleton<IMemesListViewModel>(() =>
            new MemesListViewModel(filtersObserver, resolver.GetRequiredService<IMemeService>()));
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