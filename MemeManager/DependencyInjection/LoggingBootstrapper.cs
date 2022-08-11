using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Extensions.Logging;
using Splat;

namespace MemeManager.DependencyInjection;

public static class LoggingBootstrapper
{
    public static void RegisterLogging(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        services.RegisterLazySingleton(() =>
        {
            var serilogConfig = resolver.GetRequiredService<IConfiguration>();
            var logger = new LoggerConfiguration()
                // https://github.com/serilog/serilog-sinks-file#json-appsettingsjson-configuration
                .ReadFrom.Configuration(serilogConfig)
                .CreateLogger();
            var factory = new SerilogLoggerFactory(logger);

            return factory.CreateLogger("Default");
        });
        /*
         * TODO: In the future, we could fetch the log instance in a much easier way and access shorter
         * method names than what MSoft.Extensions.Logging provides. https://www.reactiveui.net/docs/handbook/logging/
         */
    }
}