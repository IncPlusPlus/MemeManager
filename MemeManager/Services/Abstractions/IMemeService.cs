using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services.Abstractions;

public interface IMemeService
{
    IEnumerable<Meme> GetAll();

    // TODO: Expand this to tags and keywords
    IEnumerable<Meme> GetFiltered(Category? category);
    Task<List<Meme>> GetFilteredAsync(Category? category, CancellationToken token);

    Meme? GetById(int id);

    Meme Create(Meme newMeme);

    Meme? DeleteById(int id);
}