using System.ComponentModel.DataAnnotations;

namespace MakeItShort.API.Domain.Request
{
    public class ShortUrlRequest
    {
        public string Url { get; set; } = string.Empty;
    }
}