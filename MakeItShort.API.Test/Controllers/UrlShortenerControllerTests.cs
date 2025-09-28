using MakeItShort.API.Controllers;
using MakeItShort.API.Domain.Request;
using MakeItShort.API.Domain.Response;
using MakeItShort.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace MakeItShort.API.Test.Controllers
{
    public class UrlShortenerControllerTests
    {
        private readonly Mock<IUrlShortenerService> _serviceMock;
        private readonly Mock<ILogger<UrlShortenerController>> _loggerMock;
        private readonly UrlShortenerController _controller;

        public UrlShortenerControllerTests()
        {
            _serviceMock = new Mock<IUrlShortenerService>();
            _loggerMock = new Mock<ILogger<UrlShortenerController>>();
            _controller = new UrlShortenerController(_loggerMock.Object, _serviceMock.Object);
        }

        // ------------------------
        // Index
        // ------------------------
        [Fact]
        public void Index_ShouldReturnOk()
        {
            var result = _controller.Index();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("MakeItShort API Version 1.0", ok.Value);
        }

        // ------------------------
        // CreateShortUrlAsync
        // ------------------------
        [Fact]
        public async Task CreateShortUrlAsync_ShouldReturnCreated_WhenSuccessful()
        {
            var request = new ShortUrlRequest { Url = "http://valid.com" };
            var response = new ShortUrlResponse("http://short.ly/abc");

            _serviceMock.Setup(s => s.CreateShortUrlAsync(request))
                        .ReturnsAsync(response);

            var result = await _controller.CreateShortUrlAsync(request);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal(response, created.Value);
        }

        [Fact]
        public async Task CreateShortUrlAsync_ShouldReturnBadRequest_WhenArgumentException()
        {
            var request = new ShortUrlRequest { Url = "invalid" };

            _serviceMock.Setup(s => s.CreateShortUrlAsync(It.IsAny<ShortUrlRequest>()))
                        .ThrowsAsync(new ArgumentException("Invalid"));

            var result = await _controller.CreateShortUrlAsync(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Url nula ou inválida.", badRequest.Value);
        }

        [Fact]
        public async Task CreateShortUrlAsync_ShouldReturn500_WhenUnexpectedError()
        {
            var request = new ShortUrlRequest { Url = "http://valid.com" };

            _serviceMock.Setup(s => s.CreateShortUrlAsync(It.IsAny<ShortUrlRequest>()))
                        .ThrowsAsync(new Exception("boom"));

            var result = await _controller.CreateShortUrlAsync(request);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ------------------------
        // ResolveUrlAsync
        // ------------------------
        [Fact]
        public async Task ResolveUrlAsync_ShouldRedirect_WhenFound()
        {
            _serviceMock.Setup(s => s.ResolveUrlAsync("abc"))
                        .ReturnsAsync("http://valid.com");

            var result = await _controller.ResolveUrlAsync("abc");

            var redirect = Assert.IsType<RedirectResult>(result);
            Assert.Equal("http://valid.com", redirect.Url);
        }

        [Fact]
        public async Task ResolveUrlAsync_ShouldReturnNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.ResolveUrlAsync("abc"))
                        .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.ResolveUrlAsync("abc");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ResolveUrlAsync_ShouldReturn500_WhenUnexpectedError()
        {
            _serviceMock.Setup(s => s.ResolveUrlAsync("abc"))
                        .ThrowsAsync(new Exception("boom"));

            var result = await _controller.ResolveUrlAsync("abc");

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // ------------------------
        // GetUrlMetadataAsync
        // ------------------------
        [Fact]
        public async Task GetUrlMetadataAsync_ShouldReturnOk_WhenFound()
        {
            var response = new GetUrlMetadataResponse { ShortKey = "abc", OriginalUrl = "http://valid.com" };

            _serviceMock.Setup(s => s.GetUrlMetadataAsync("abc"))
                        .ReturnsAsync(response);

            var result = await _controller.GetUrlMetadataAsync("abc");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, ok.Value);
        }

        [Fact]
        public async Task GetUrlMetadataAsync_ShouldReturnNotFound_WhenMissing()
        {
            _serviceMock.Setup(s => s.GetUrlMetadataAsync("abc"))
                        .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetUrlMetadataAsync("abc");

            Assert.IsType<NotFoundResult>(result);
        }

        // ------------------------
        // DeleteUrl
        // ------------------------
        [Fact]
        public async Task DeleteUrl_ShouldReturnNoContent_WhenDeleted()
        {
            _serviceMock.Setup(s => s.DeleteUrlAsync("abc"))
                        .Returns(Task.CompletedTask);

            var result = await _controller.DeleteUrl("abc");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUrl_ShouldReturnBadRequest_WhenInvalidKey()
        {
            _serviceMock.Setup(s => s.DeleteUrlAsync("abc"))
                        .ThrowsAsync(new ArgumentException("invalid"));

            var result = await _controller.DeleteUrl("abc");

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("invalid", bad.Value);
        }

        [Fact]
        public async Task DeleteUrl_ShouldReturnNoContent_WhenKeyNotFound()
        {
            _serviceMock.Setup(s => s.DeleteUrlAsync("abc"))
                        .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.DeleteUrl("abc");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteUrl_ShouldReturn500_WhenUnexpectedError()
        {
            _serviceMock.Setup(s => s.DeleteUrlAsync("abc"))
                        .ThrowsAsync(new Exception("boom"));

            var result = await _controller.DeleteUrl("abc");

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}
