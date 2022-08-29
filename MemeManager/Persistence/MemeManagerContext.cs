using System;
using System.IO;
using MemeManager.Persistence.Entity;
using Microsoft.EntityFrameworkCore;

namespace MemeManager.Persistence;

public sealed class MemeManagerContext : DbContext
{
    public MemeManagerContext()
    {
        // https://stackoverflow.com/a/50042017/1687436
        // Console.WriteLine("New database created: "+ Database.EnsureCreated());

        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData,
            Environment.SpecialFolderOption.DoNotVerify);
        // Ensure the directory and all its parents exist.
        Directory.CreateDirectory(path);
        DbPath = System.IO.Path.Join(path, "MemeManager.db");
    }

    public DbSet<Meme> Memes { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Category> Categories { get; set; }

    public string DbPath { get; }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            // See https://docs.microsoft.com/en-us/ef/core/querying/related-data/lazy
            .UseLazyLoadingProxies()
            .UseSqlite($"Data Source={DbPath}");
}

/*
 * The below factory has been removed because I've reverted back to the simple context constructor. This is because I've
 * realized that using the DI system to send an instance of the context has a nonzero chance of causing multiple threads
 * to access the same DbContext instance.
 * See https://docs.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues for more
 * info on the issue. For now I've decided to just have my classes be tightly coupled with this class. It's not the
 * best feeling but it will help me avoid really confusing bugs.
 */

/// <summary>
/// Because I want the MemeManagerContext to be passed using Dependency Injection, I also want it to be able to have
/// configuration options in its constructor. However, I still need to use a pattern supported at design time so that
/// I can run commands like "dotnet ef migrations add AddNewPropToClass" and "dotnet ef database update".
///
/// The factory pattern lets me keep EF happy while also making the MemeManagerContext class flexible enough to allow
/// for configuration when using dependency injection library.
/// See https://docs.microsoft.com/en-us/ef/core/cli/dbcontext-creation?tabs=dotnet-core-cli#from-a-design-time-factory.
///
/// This class is only used at design-time by the Entity Framework. The MemeManagerContext class constructor is used
/// at runtime. To specify the argument when using the dotnet CLI ef commands, use two dashes. The -- token directs
/// dotnet ef to treat everything that follows as an argument and not try to parse them as options.
/// Any extra arguments not used by dotnet ef are forwarded to the app.
/// See https://docs.microsoft.com/en-us/ef/core/cli/dotnet#aspnet-core-environment.
/// </summary>
// public class MemeManagerContextFactory : IDesignTimeDbContextFactory<MemeManagerContext>
// {
//     public MemeManagerContext CreateDbContext(string[] args)
//     {
//         var optionsBuilder = new DbContextOptionsBuilder<MemeManagerContext>();
//         string dbname = "memes.db";
//         if (args.Length > 0)
//         {
//             Console.WriteLine("Meme db name set to: " + args[0]);
//             dbname = args[0];
//         }
//         /*
//          * TODO: Might want to change from LocalApplicationData to a more permanent location sorta like what
//          * ConfigurationBootstrapper.GetDatabaseConnectionString does.
//          */
//         var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.DoNotVerify);
//         // Ensure the directory and all its parents exist.
//         Directory.CreateDirectory(path);
//         var DbPath = System.IO.Path.Join(path, dbname);
//         
//         optionsBuilder
//             // .UseLazyLoadingProxies()
//             .UseSqlite($"Data Source={DbPath}");
//
//         return new MemeManagerContext(optionsBuilder.Options);
//     }
// }
