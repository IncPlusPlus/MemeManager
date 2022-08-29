using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IMemeService
{
    IEnumerable<Meme> GetAll();

    IEnumerable<Meme> GetFiltered(Category? category, string? searchTerms);
    Task<List<Meme>> GetFilteredAsync(Category? category, string? searchTerms, CancellationToken token);

    Meme? GetById(int id);

    Meme Create(Meme newMeme);

    Meme? DeleteById(int id);
}
