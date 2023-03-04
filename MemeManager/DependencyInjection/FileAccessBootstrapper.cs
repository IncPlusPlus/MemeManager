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
        var logger = resolver.GetRequiredService<ILogger>();
        // Grab an instance of the change notifier. This is an awful hacky way around doing reactive data properly. I'll figure it out at some point...
        var dbChangeNotifier = resolver.GetRequiredService<IDbChangeNotifier>();
        var importRequestNotifier = resolver.GetRequiredService<IImportRequestNotifier>();
        var memeService = resolver.GetRequiredService<IMemeService>();
        var categoryService = resolver.GetRequiredService<ICategoryService>();
        var tagService = resolver.GetRequiredService<ITagService>();
        services.RegisterLazySingleton<IImportService>(() => new ImportService(logger, dbChangeNotifier, importRequestNotifier, memeService, categoryService, tagService));
    }
}
