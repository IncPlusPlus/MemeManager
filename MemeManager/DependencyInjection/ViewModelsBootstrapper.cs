using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
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
        services.RegisterLazySingleton<ICategoriesListViewModel>(() =>
            new CategoriesListViewModel(filtersObserver, resolver.GetRequiredService<ICategoryService>()));
        services.RegisterLazySingleton<IMemesListViewModel>(() =>
            new MemesListViewModel(filtersObserver, resolver.GetRequiredService<IMemeService>()));
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ICategoriesListViewModel>(),
            resolver.GetRequiredService<IMemesListViewModel>()
        ));
    }

    private static void RegisterFactories(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
    }
}