using MemeManager.Persistence;
using MemeManager.ViewModels.Implementations;
using MemeManager.ViewModels.Interfaces;
using Microsoft.EntityFrameworkCore;
using Splat;

namespace MemeManager.DependencyInjection;

public static class ViewModelsBootstrapper
{
    public static void RegisterViewModels(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterServices(services, resolver);
        RegisterFactories(services, resolver);
    }

    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<ICategoriesListViewModel>(() => new CategoriesListViewModel());
        services.RegisterLazySingleton<IMemesListViewModel>(() => new MemesListViewModel());
        services.RegisterLazySingleton<IMainWindowViewModel>(() => new MainWindowViewModel(
            resolver.GetRequiredService<ICategoriesListViewModel>(),
            resolver.GetRequiredService<IMemesListViewModel>()
        ));
    }

    private static void RegisterFactories(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
    }
}