using System;
using System.IO;
using MemeManager.Persistence;
using Microsoft.EntityFrameworkCore;
using Splat;

namespace MemeManager.DependencyInjection;

public class DataAccessBootstrapper
{
    public static void RegisterDataAccess(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        RegisterDatabaseConnectionOptions(services);
    }

    private static void RegisterDatabaseConnectionOptions(IMutableDependencyResolver services)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MemeManagerContext>();
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
        // Ensure the directory and all its parents exist.
        Directory.CreateDirectory(path);
        var dbPath = Path.Join(path, "MemeManager.db");
        
        optionsBuilder.UseSqlite($"Data Source={dbPath}");

        services.RegisterLazySingleton(() => optionsBuilder.Options);
    }
}