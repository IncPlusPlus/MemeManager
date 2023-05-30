using System.Linq;
using MemeManager.Persistence;
using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using MemeManager.ViewModels.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.DependencyInjection;

public class DataAccessBootstrapper
{
    public static void RegisterDataAccess(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // RegisterDatabaseConnectionOptions(services);
        RegisterNotifiers(services, resolver);
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

    private static void RegisterNotifiers(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton<IDbChangeNotifier>(() => new DbChangeNotifier());
        services.RegisterLazySingleton<IImportRequestNotifier>(() => new ImportRequestNotifier());
    }

    private static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var logger = resolver.GetRequiredService<ILogger>();
        // Grab an instance of the change notifier. This is an awful hacky way around doing reactive data properly. I'll figure it out at some point...
        var dbChangeNotifier = resolver.GetRequiredService<IDbChangeNotifier>();
        // I know that Entity Framework is supposed to be used in the opposite way to this and have a unique instance per request.
        // However, that's a pain in the ass and I don't want to do it properly yet. I kept running into https://stackoverflow.com/a/48204159/1687436 when I was making a new MemeManagerContext() for each repository.
        // This is absolutely NOT how Entity Framework should be used. See https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#the-dbcontext-lifetime
        var dbContext = new MemeManagerContext();
        var numPendingMigrations = dbContext.Database.GetPendingMigrations().Count();
        if (numPendingMigrations > 0)
        {
            logger.LogInformation("The database has {NumMigrations} pending migration(s). Attempting to apply them now",
                numPendingMigrations);
            dbContext.Database.Migrate();
            logger.LogInformation("Applied migrations successfully");
        }

        services.RegisterLazySingleton<ITagService>(() => new TagService(dbContext, dbChangeNotifier, logger));
        services.RegisterLazySingleton<ICategoryService>(() =>
            new CategoryService(dbContext, dbChangeNotifier, logger));
        services.RegisterLazySingleton<IMemeService>(() => new MemeService(dbContext, dbChangeNotifier, logger));
        services.RegisterConstant<ILifecycleService>(new LifecycleService(logger,
            resolver.GetRequiredService<MemesConfiguration>(), resolver.GetRequiredService<LayoutConfiguration>()));
    }
}
