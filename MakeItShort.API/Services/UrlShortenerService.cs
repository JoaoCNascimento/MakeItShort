using MakeItShort.API.Domain.Request;
using MakeItShort.API.Domain.Response;
using MakeItShort.API.Domain.Model;
using MakeItShort.API.Repository.Interfaces;
using MakeItShort.API.Services.Interfaces;
using MakeItShort.API.Utils;

namespace MakeItShort.API.Services;

public class UrlShortenerService(IUrlShortenerRepository repository, IConfiguration configuration) : IUrlShortenerService
{
    private readonly IUrlShortenerRepository _repository = repository;
    private readonly string _baseUrl = configuration["Shortener:BaseUrl"] ?? throw new ArgumentException("Base Url variable should not be null");

    private static void ValidateUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
            (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Invalid URL format. Only HTTP/HTTPS URLs are supported.", nameof(url));
    }

    private async Task<string> GenerateUniqueShortKeyAsync(int length = 7)
    {
        string shortKey;
        bool exists;
        do
        {
            shortKey = ShortKeyGeneratorUtil.GenerateShortKey(length);
            exists = await _repository.CheckIfShortKeyAlreadyExists(shortKey);
        } 
        while (exists);

        return shortKey;
    }

    public async Task<ShortUrlResponse> CreateShortUrlAsync(ShortUrlRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Url))
            throw new ArgumentException("URL request cannot be null or empty.");

        ValidateUrl(request.Url);
        // Deduplication: Check if URL already exists and returns it
        var existing = await _repository.GetShortUrlByOriginalAsync(request.Url);
        if (existing is not null) return new ShortUrlResponse($"{_baseUrl}/{existing.ShortKey}");

        string shortKey = await GenerateUniqueShortKeyAsync();

        var shortUrl = new ShortUrl
        {
            ShortKey = shortKey,
            OriginalUrl = request.Url
        };

        var result = await _repository.CreateShortUrlAsync(shortUrl);

        return new ShortUrlResponse($"{_baseUrl}/{result.ShortKey}");
    }

    public async Task DeleteUrlAsync(string shortKey)
    {
        if (string.IsNullOrWhiteSpace(shortKey))
        throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

        var deletedRows = await _repository.DeleteUrlAsync(shortKey);
        if (deletedRows == 0)
            throw new KeyNotFoundException($"No URL found with short key {shortKey}.");
    }

    public async Task<GetUrlMetadataResponse> GetUrlMetadataAsync(string shortKey)
    {
        if (string.IsNullOrWhiteSpace(shortKey))
            throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

        var shortUrl = await _repository.GetShortUrlAsync(shortKey) ?? throw new KeyNotFoundException($"No URL found with short key {shortKey}.");

        return new GetUrlMetadataResponse
        {
            ShortKey = shortUrl.ShortKey,
            OriginalUrl = shortUrl.OriginalUrl,
            Hits = shortUrl.Hits,
            CreatedAt = shortUrl.CreatedAt,
            UpdatedAt = shortUrl.UpdatedAt
        };
    }

    public async Task<string> ResolveUrlAsync(string shortKey)
    {
        if (string.IsNullOrWhiteSpace(shortKey))
            throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

        var shortUrl = await _repository.GetShortUrlAsync(shortKey) 
            ?? throw new KeyNotFoundException($"No URL found with short key {shortKey}.");

        _ = _repository.UpdateHitCountAsync(shortKey).ContinueWith(task =>
        {
            if (!task.Result)
                throw new InvalidOperationException($"Failed to update hit count for short key {shortKey}.");
        }, TaskContinuationOptions.OnlyOnFaulted);

        return shortUrl.OriginalUrl;
    }
}