using MemeManager.Persistence;
using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using MemeManager.ViewModels.Configuration;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.DependencyInjection;

public class DataAccessBootstrapper
{
    public static void RegisterDataAccess(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // RegisterDatabaseConnectionOptions(services);
        RegisterServices(services, resolver);
    }

    // private static void RegisterDatabaseConnectionOptions(IMutableDependencyResolver services)
    // {
    //     var optionsBuilder = new DbContextOptionsBuilder<MemeManagerContext>();
    //     var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
    //     // Ensure the directory and all its parents exist.
    //     Directory.CreateDirectory(path);
    //     var dbPath = Path.Join(path, "MemeManager.db");
    //     
    //     optionsBuilder.UseSqlite($"Data Source={dbPath}");
    //
    //     services.RegisterLazySingleton(() => optionsBuilder.Options);
    // }

    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<ICategoryService>(() => new CategoryService(new MemeManagerContext(),resolver.GetRequiredService<ILogger>()));
        services.RegisterLazySingleton<IMemeService>(() => new MemeService(new MemeManagerContext(),resolver.GetRequiredService<ILogger>() ));
        services.RegisterConstant<ILifecycleService>(new LifecycleService(resolver.GetRequiredService<ILogger>(), resolver.GetRequiredService<MemesConfiguration>(), resolver.GetRequiredService<LayoutConfiguration>()));
    }
}