using MemeManager.Services.Environment.Enums;
using MemeManager.Services.Environment.Implementations;
using MemeManager.Services.Environment.Interfaces;
using Splat;

namespace MemeManager.DependencyInjection;

public static class EnvironmentServicesBootstrapper
{
    public static void RegisterEnvironmentServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterCommonServices(services);
        RegisterPlatformSpecificServices(services, resolver);
    }

    private static void RegisterCommonServices(IMutableDependencyResolver services)
    {
        // services.RegisterLazySingleton<IEnvironmentService>(() => new EnvironmentService());
        // services.RegisterLazySingleton<IProcessService>(() => new ProcessService());
        // services.RegisterLazySingleton<IEnvironmentFileService>(() => new EnvironmentFileService());
        // services.RegisterLazySingleton<IEnvironmentDirectoryService>(() => new EnvironmentDirectoryService());
        // services.RegisterLazySingleton<IEnvironmentDriveService>(() => new EnvironmentDriveService());
        // services.RegisterLazySingleton<IEnvironmentPathService>(() => new EnvironmentPathService());
        // services.RegisterLazySingleton<IRegexService>(() => new RegexService());
        services.Register<IPlatformService>(() => new PlatformService());
    }

    private static void RegisterPlatformSpecificServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var platformService = resolver.GetRequiredService<IPlatformService>();
        var platform = platformService.GetPlatform();

        if (platform is Platform.Windows)
        {
            // RegisterWindowsServices(services, resolver);
        }
    }
}