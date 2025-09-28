using MakeItShort.API.Domain.Model;
using MakeItShort.API.Domain.Request;
using MakeItShort.API.Repository.Interfaces;
using MakeItShort.API.Services;
using Microsoft.Extensions.Configuration;
using Moq;

namespace MakeItShort.API.Test.Services
{
    public class UrlShortenerServiceTests
    {
        private readonly Mock<IUrlShortenerRepository> _repositoryMock;
        private readonly UrlShortenerService _service;

        public UrlShortenerServiceTests()
        {
            _repositoryMock = new Mock<IUrlShortenerRepository>();

            var inMemorySettings = new Dictionary<string, string> {
                {"Shortener:BaseUrl", "http://short.ly"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new UrlShortenerService(_repositoryMock.Object, configuration);
        }

        // ------------------------
        // CreateShortUrlAsync
        // ------------------------
        [Fact]
        public async Task CreateShortUrlAsync_ShouldThrow_WhenRequestIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShortUrlAsync(null));
        }

        [Fact]
        public async Task CreateShortUrlAsync_ShouldThrow_WhenUrlIsInvalid()
        {
            var request = new ShortUrlRequest { Url = "invalid-url" };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateShortUrlAsync(request));
        }

        [Fact]
        public async Task CreateShortUrlAsync_ShouldCreateNewShortUrl_WhenNotExists()
        {
            var request = new ShortUrlRequest { Url = "http://valid.com" };

            _repositoryMock.Setup(r => r.GetShortUrlByOriginalAsync(request.Url))
                .ReturnsAsync(null as ShortUrl);
            _repositoryMock.Setup(r => r.CheckIfShortKeyAlreadyExists(It.IsAny<string>()))
                .ReturnsAsync(false);
            _repositoryMock.Setup(r => r.CreateShortUrlAsync(It.IsAny<ShortUrl>()))
                .ReturnsAsync((ShortUrl s) => s);

            var result = await _service.CreateShortUrlAsync(request);

            Assert.StartsWith("http://short.ly/", result.Url);
        }

        // ------------------------
        // DeleteUrlAsync
        // ------------------------

        [Fact]
        public async Task DeleteUrlAsync_ShouldThrow_WhenShortKeyIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteUrlAsync(null));
        }

        [Fact]
        public async Task DeleteUrlAsync_ShouldSucced_WhenNoRowsDeleted()
        {
            _repositoryMock.Setup(r => r.DeleteUrlAsync("abc"))
                .ReturnsAsync(0);

            await _service.DeleteUrlAsync("abc");
        }

        [Fact]
        public async Task DeleteUrlAsync_ShouldSucceed_WhenDeleted()
        {
            _repositoryMock.Setup(r => r.DeleteUrlAsync("abc"))
                .ReturnsAsync(1);

            await _service.DeleteUrlAsync("abc");
        }

        // ------------------------
        // GetUrlMetadataAsync
        // ------------------------

        [Fact]
        public async Task GetUrlMetadataAsync_ShouldThrow_WhenShortKeyIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetUrlMetadataAsync(null));
        }

        [Fact]
        public async Task GetUrlMetadataAsync_ShouldThrow_WhenNotFound()
        {
            _repositoryMock.Setup(r => r.GetShortUrlAsync("abc"))
                .ReturnsAsync(null as ShortUrl);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetUrlMetadataAsync("abc"));
        }

        [Fact]
        public async Task GetUrlMetadataAsync_ShouldReturnMetadata_WhenFound()
        {
            var shortUrl = new ShortUrl
            {
                ShortKey = "abc",
                OriginalUrl = "http://valid.com",
                Hits = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow
            };

            _repositoryMock.Setup(r => r.GetShortUrlAsync("abc"))
                .ReturnsAsync(shortUrl);

            var result = await _service.GetUrlMetadataAsync("abc");

            Assert.Equal(shortUrl.ShortKey, result.ShortKey);
            Assert.Equal(shortUrl.OriginalUrl, result.OriginalUrl);
            Assert.Equal(shortUrl.Hits, result.Hits);
        }

        // ------------------------
        // ResolveUrlAsync
        // ------------------------
        [Fact]
        public async Task ResolveUrlAsync_ShouldThrow_WhenShortKeyIsNull()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ResolveUrlAsync(null));
        }

        [Fact]
        public async Task ResolveUrlAsync_ShouldThrow_WhenNotFound()
        {
            _repositoryMock.Setup(r => r.GetShortUrlAsync("abc"))
                .ReturnsAsync(null as ShortUrl);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.ResolveUrlAsync("abc"));
        }

        [Fact]
        public async Task ResolveUrlAsync_ShouldReturnOriginalUrl_AndTriggerUpdate_WhenSuccessful()
        {
            var shortUrl = new ShortUrl { ShortKey = "abc", OriginalUrl = "http://valid.com" };

            _repositoryMock.Setup(r => r.GetShortUrlAsync("abc"))
                .ReturnsAsync(shortUrl);
            _repositoryMock.Setup(r => r.UpdateHitCountAsync("abc"))
                .ReturnsAsync(true);

            var result = await _service.ResolveUrlAsync("abc");

            Assert.Equal(shortUrl.OriginalUrl, result);
            _repositoryMock.Verify(r => r.UpdateHitCountAsync("abc"), Times.Once());
        }

        [Fact]
        public async Task ResolveUrlAsync_ShouldReturnOriginalUrl_EvenIfUpdateFails()
        {
            var shortUrl = new ShortUrl { ShortKey = "abc", OriginalUrl = "http://valid.com" };

            _repositoryMock.Setup(r => r.GetShortUrlAsync("abc"))
                .ReturnsAsync(shortUrl);
            _repositoryMock.Setup(r => r.UpdateHitCountAsync("abc"))
                .ReturnsAsync(false);

            var result = await _service.ResolveUrlAsync("abc");

            Assert.Equal(shortUrl.OriginalUrl, result);
            _repositoryMock.Verify(r => r.UpdateHitCountAsync("abc"), Times.Once());
        }
    }
}
