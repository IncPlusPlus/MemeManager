using System.Collections.Generic;
using System.Threading.Tasks;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IImportService
{
    public IEnumerable<Meme> ImportFromDirectory(string path);
    Task GenerateThumbnails(IEnumerable<Meme> importedMemes);
}
