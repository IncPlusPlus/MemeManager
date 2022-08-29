using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MemeManager.Persistence.Entity;

namespace MemeManager.Services;

public class Database
{
    // public IEnumerable<Meme> GetMemes() => new MemeManagerContext().Memes.ToList();
    public IEnumerable<Meme> GetMemes() => new[]
    {
        new Meme{Id=1,Category = new Category{Id = 1,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=2,Category = new Category{Id = 2,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=3,Category = new Category{Id = 3,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=4,Category = new Category{Id = 4,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=5,Category = new Category{Id = 5,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=6,Category = new Category{Id = 6,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=7,Category = new Category{Id = 7,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=8,Category = new Category{Id = 8,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=9,Category = new Category{Id = 9,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=10,Category = new Category{Id = 10,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=11,Category = new Category{Id = 11,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=12,Category = new Category{Id = 12,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=13,Category = new Category{Id = 13,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=14,Category = new Category{Id = 14,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=15,Category = new Category{Id = 15,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=16,Category = new Category{Id = 16,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=17,Category = new Category{Id = 17,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=18,Category = new Category{Id = 18,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=19,Category = new Category{Id = 19,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
        new Meme{Id=20,Category = new Category{Id = 20,Name = "Shitpost", Path = "somepath"}, Name = "ShitPostName", Path = "somePath", AdditionalTerms = "",Tags = {},MediaType = Meme.FileMediaType.Image,TimeAdded = DateTime.Now, CachedThumbnailPath = "C:\\Users\\Ryan Cloherty\\Downloads\\MyMemes\\thumbnails\\jblogo (2).png"},
    };

    public async Task<IEnumerable<Meme>> GetMemesAsync()
    {
        return await Task.FromResult(GetMemes());
    }
}
