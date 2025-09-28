using MakeItShort.API.Domain.Model;

namespace MakeItShort.API.Repository.Interfaces;

public interface IUrlShortenerRepository
{
    Task<ShortUrl> CreateShortUrlAsync(ShortUrl shortUrl);
    Task<ShortUrl?> GetShortUrlAsync(string shortKey);
    Task<int> DeleteUrlAsync(string shortKey);
    Task<bool> UpdateHitCountAsync(string shortKey);
    Task<ShortUrl?> GetShortUrlByOriginalAsync(string originalUrl);
    Task<bool> CheckIfShortKeyAlreadyExists(string shortKey);
}