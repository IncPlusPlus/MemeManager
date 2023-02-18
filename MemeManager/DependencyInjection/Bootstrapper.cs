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
        DataAccessBootstrapper.RegisterDataAccess(services, resolver);
        FileAccessBootstrapper.RegisterFileAccess(services, resolver);
        ViewModelsBootstrapper.RegisterViewModels(services, resolver);
    }
}
