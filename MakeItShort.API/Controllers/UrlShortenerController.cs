using System.Threading.Tasks;
using MakeItShort.API.Domain.Request;
using MakeItShort.API.Domain.Response;
using MakeItShort.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MakeItShort.API.Controllers;
[Route("api/urls")]
[ApiController]
public class UrlShortenerController : ControllerBase
{
    private readonly ILogger<UrlShortenerController> _logger;
    private readonly IUrlShortenerService _service;

    public UrlShortenerController(ILogger<UrlShortenerController> logger, IUrlShortenerService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        return Ok("MakeItShort API Version 1.0");
    }

    [HttpPost]
    public async Task<IActionResult> CreateShortUrlAsync(ShortUrlRequest request)
    {
        try
        {
            ShortUrlResponse response = await _service.CreateShortUrlAsync(request);
            return Created(nameof(GetUrlMetadataAsync), response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("{errorMessage}", ex.Message);
            return BadRequest("Url nula ou inválida.");
        }
        catch (Exception ex)
        {
            string errorMessage = "An error has ocurred while creating the short url.";
            _logger.LogError($"{errorMessage} {ex.Message}", ex);
            return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
        }
    }

    [HttpGet("/{shortKey}")]
    public async Task<IActionResult> ResolveUrlAsync([FromRoute] string shortKey)
    {
        try
        {
            string url = await _service.ResolveUrlAsync(shortKey);
            return Redirect(url);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            string message = "An error has ocurred while resolving the short url";
            _logger.LogError("{message}. {errorMessage}", message, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    [HttpGet("{shortKey}")]
    public async Task<IActionResult> GetUrlMetadataAsync([FromRoute] string shortKey)
    {
        try
        {
            GetUrlMetadataResponse response = await _service.GetUrlMetadataAsync(shortKey);
            return Ok(response);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            string message = "Houve um erro ao recuperar os dados da URL.";
            _logger.LogError("{message} {errorMessage}", message, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }

    [HttpDelete("{shortKey}")]
    public async Task<IActionResult> DeleteUrl(string shortKey)
    {
        try
        {
            await _service.DeleteUrlAsync(shortKey);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            string message = "An error has ocurred while deleting the resource";
            _logger.LogError("{message}", message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}