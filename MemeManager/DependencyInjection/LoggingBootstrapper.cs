using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Splat;

namespace MemeManager.DependencyInjection;

public static class LoggingBootstrapper
{
    public static void RegisterLogging(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        // Register an ILoggerFactory instance for inflexible APIs that might require an ILoggerFactory instead of an ILogger instance
        services.RegisterLazySingleton(() =>
        {
            var globalConfig = resolver.GetRequiredService<IConfiguration>();
            var logger = new LoggerConfiguration()
                /*
                 * This is needed because the actual "Serilog" key needs to be present for LoggerConfiguration.ReadFrom
                 * .Configuration(serilogConfig).CreateLogger() to work properly. I can't just use configuration.GetRequiredSection.
                 * The method below that does get that section is purely for keeping it in memory as a POCO so that it can be
                 * written back to the config YAML when we want to save the config values.
                 */
                // https://github.com/serilog/serilog-sinks-file#json-appsettingsjson-configuration
                .ReadFrom.Configuration(globalConfig)
                .CreateLogger();
            return (ILoggerFactory)new SerilogLoggerFactory(logger);
        });
        // Register a default ILogger instance
        services.RegisterLazySingleton(() => resolver.GetRequiredService<ILoggerFactory>().CreateLogger("Default"));
        /*
         * TODO: In the future, we could fetch the log instance in a much easier way and access shorter
         * method names than what MSoft.Extensions.Logging provides. https://www.reactiveui.net/docs/handbook/logging/
         */
    }
}
