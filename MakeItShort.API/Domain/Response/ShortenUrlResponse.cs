namespace MakeItShort.API.Domain.Response;
public class ShortUrlResponse(string url)
{
    public string Url { get; set; } = url;
}