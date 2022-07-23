using MemeManager.Persistence.Entity;

namespace EFGetStarted.Persistence;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class MemeManagerContext : DbContext
{
    public DbSet<Meme> Memes { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public string DbPath { get; }

    public MemeManagerContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "memes.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");
}