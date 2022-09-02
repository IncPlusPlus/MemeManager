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
        // I know that Entity Framework is supposed to be used in the opposite way to this and have a unique instance per request.
        // However, that's a pain in the ass and I don't want to do it properly yet. I kept running into https://stackoverflow.com/a/48204159/1687436 when I was making a new MemeManagerContext() for each repository.
        // This is absolutely NOT how Entity Framework should be used. See https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#the-dbcontext-lifetime
        var dbContext = new MemeManagerContext();
        services.RegisterLazySingleton<ICategoryService>(() => new CategoryService(dbContext, resolver.GetRequiredService<ILogger>()));
        services.RegisterLazySingleton<IMemeService>(() => new MemeService(dbContext, resolver.GetRequiredService<ILogger>()));
        services.RegisterConstant<ILifecycleService>(new LifecycleService(resolver.GetRequiredService<ILogger>(), resolver.GetRequiredService<MemesConfiguration>(), resolver.GetRequiredService<LayoutConfiguration>()));
    }
}
