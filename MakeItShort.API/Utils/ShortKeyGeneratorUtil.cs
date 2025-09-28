namespace MakeItShort.API.Utils;
public class ShortKeyGeneratorUtil
{
    private const string Chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly Random Random = new();

    public static string GenerateShortKey(int length = 7)
    {
        // Generate a random char array that is converted into a string
        return new string([.. Enumerable.Repeat(Chars, length).Select(s => s[Random.Next(s.Length)])]);
    }
}
