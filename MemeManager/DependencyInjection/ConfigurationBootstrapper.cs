using System;
using MemeManager.ViewModels.Configuration;
using Microsoft.Extensions.Configuration;
using Splat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MemeManager.DependencyInjection;

public static class ConfigurationBootstrapper
{
    public static void RegisterConfiguration(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        var configuration = BuildConfiguration();
        
        // Required for Serilog to get the portion of the config that starts with the Serilog key
        RegisterGlobalAppSettings(services, configuration);
        // Register class instances that represent a section of the config
        RegisterAboutDialogConfiguration(services, configuration);
        RegisterMemesConfiguration(services, configuration);
        
    }

    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddYamlFile("appsettings.yaml")
            .Build();

    private static void RegisterGlobalAppSettings(IMutableDependencyResolver services, IConfiguration configuration)
    {
        services.RegisterConstant(configuration);
    }

    private static void RegisterAboutDialogConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        services.RegisterConstant(configuration.GetRequiredSection("About").Get<AboutDialogConfiguration>());
    }

    private static void RegisterMemesConfiguration(IMutableDependencyResolver services, IConfiguration configuration)
    {
        services.RegisterConstant(configuration.GetRequiredSection("Memes").Get<MemesConfiguration>());
    }
}