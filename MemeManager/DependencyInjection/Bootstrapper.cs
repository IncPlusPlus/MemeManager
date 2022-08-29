using Splat;

namespace MemeManager.DependencyInjection;

public class Bootstrapper
{
    public static void Register(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        EnvironmentServicesBootstrapper.RegisterEnvironmentServices(services, resolver);
        ConfigurationBootstrapper.RegisterConfiguration(services, resolver);
        LoggingBootstrapper.RegisterLogging(services, resolver);
        // AvaloniaServicesBootstrapper.RegisterAvaloniaServices(services);
        // FileSystemWatcherServicesBootstrapper.RegisterFileSystemWatcherServices(services, resolver);
        DataAccessBootstrapper.RegisterDataAccess(services, resolver);
        // ServicesBootstrapper.RegisterServices(services, resolver);
        ViewModelsBootstrapper.RegisterViewModels(services, resolver);
    }
}
