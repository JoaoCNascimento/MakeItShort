namespace MakeItShort.API.Domain.Response;

public record GetUrlMetadataResponse
{
    public string ShortKey { get; set; } = string.Empty;
    public int Hits { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}