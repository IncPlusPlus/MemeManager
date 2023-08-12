using System.Collections.Generic;
using System.Threading.Tasks;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IImportService
{
    /// <summary>
    /// Begins the import process for memes at the specified path
    /// </summary>
    /// <param name="path">the path to a folder containing memes</param>
    void ImportFromDirectory(string path);

    /// <summary>
    /// Called from the main window's ViewModel. PLEASE NEVER CALL THIS METHOD YOURSELF. This is a hacky workaround
    /// to perform all Entity Framework database operations on the UI thread to keep EF happy.
    /// </summary>
    /// <param name="basePath">the folder that was selected during the import screen</param>
    void ImportFromPaths(string basePath);

    Task GenerateThumbnails(IEnumerable<Meme> importedMemes);
    void SetThumbnails(IEnumerable<(Meme, string?)> memesAndThumbnailPaths);
}
