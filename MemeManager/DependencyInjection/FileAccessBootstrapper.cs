using MemeManager.Persistence;
using MemeManager.Services.Abstractions;
using MemeManager.Services.Implementations;
using Splat;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MemeManager.DependencyInjection;

public class FileAccessBootstrapper
{
    public static void RegisterFileAccess(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterImportService(services, resolver);
        // RegisterFileSystemWatcherServices(services, resolver);
    }

    private static void RegisterImportService(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // Use a separate instance of MemeManagerContext as we'll be interacting with Entity Framework from a thread
        // other than the main thread. If we reused the instance that the UI uses, EF will have an aneurysm.
        var altContext = new MemeManagerContext();
        var logger = resolver.GetRequiredService<ILogger>();
        // Grab an instance of the change notifier. This is an awful hacky way around doing reactive data properly.
        // I'll figure it out at some point...
        var dbChangeNotifier = resolver.GetRequiredService<IDbChangeNotifier>();
        var importRequestNotifier = resolver.GetRequiredService<IImportRequestNotifier>();
        var memeService = new MemeService(altContext, dbChangeNotifier, logger);
        var statusService = resolver.GetRequiredService<IStatusService>();
        var categoryService = new CategoryService(altContext, dbChangeNotifier, logger);
        var tagService = new TagService(altContext, dbChangeNotifier, logger);
        services.RegisterLazySingleton<IImportService>(() => new ImportService(logger, dbChangeNotifier,
            importRequestNotifier, statusService, memeService, categoryService, tagService));
    }
}
