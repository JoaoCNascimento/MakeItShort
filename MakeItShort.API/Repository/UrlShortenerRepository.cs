using MakeItShort.API.Domain.Model;
using MakeItShort.API.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeItShort.API.Repository
{
    public class UrlShortenerRepository(MakeItShortDbContext context) : IUrlShortenerRepository
    {
        private readonly MakeItShortDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

        public async Task<ShortUrl> CreateShortUrlAsync(ShortUrl shortUrl)
        {
            ArgumentNullException.ThrowIfNull(shortUrl);

            try
            {
                await _context.ShortUrls.AddAsync(shortUrl);
                await _context.SaveChangesAsync();
                return shortUrl;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException("Failed to create shortened URL.", ex);
            }
        }

        public async Task<int> DeleteUrlAsync(string shortKey)
        {
            if (string.IsNullOrWhiteSpace(shortKey))
                throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

            try
            {
                var deletedRows = await _context.ShortUrls
                    .Where(e => e.ShortKey == shortKey)
                    .ExecuteDeleteAsync();
                return deletedRows;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException($"Failed to delete shortened URL with key {shortKey}.", ex);
            }
        }

        public async Task<ShortUrl?> GetShortUrlAsync(string shortKey)
        {
            if (string.IsNullOrWhiteSpace(shortKey))
                throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

            return await _context.ShortUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ShortKey == shortKey);
        }

        public async Task<ShortUrl?> GetShortUrlByOriginalAsync(string originalUrl)
        {
            if (string.IsNullOrWhiteSpace(originalUrl))
                throw new ArgumentException("Original Url cannot be null or empty.", nameof(originalUrl));

            return await _context.ShortUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.OriginalUrl == originalUrl);
        }

        public async Task<bool> UpdateHitCountAsync(string shortKey)
        {
            if (string.IsNullOrWhiteSpace(shortKey))
                throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

            try
            {
                var updatedRows = await _context.ShortUrls
                    .Where(e => e.ShortKey == shortKey)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.Hits, e => e.Hits + 1));

                return updatedRows > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new DbUpdateException($"Failed to update hit count for shortened URL with key {shortKey}.", ex);
            }
        }

        public async Task<bool> CheckIfShortKeyAlreadyExists(string shortKey)
        {
            if (string.IsNullOrWhiteSpace(shortKey))
                throw new ArgumentException("Short key cannot be null or empty.", nameof(shortKey));

            return await _context.ShortUrls.AsNoTracking().AnyAsync(e => e.ShortKey.Equals(shortKey));
        }
    }
}