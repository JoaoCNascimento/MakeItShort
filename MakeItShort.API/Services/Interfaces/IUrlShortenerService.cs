using MakeItShort.API.Domain.Request;
using MakeItShort.API.Domain.Response;

namespace MakeItShort.API.Services.Interfaces;

public interface IUrlShortenerService
{
    public Task<ShortUrlResponse> CreateShortUrlAsync(ShortUrlRequest request);
    public Task DeleteUrlAsync(string shortKey);
    public Task<string> ResolveUrlAsync(string shortKey);
    public Task<GetUrlMetadataResponse> GetUrlMetadataAsync(string shortKey); 
}