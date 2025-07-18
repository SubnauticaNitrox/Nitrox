using System.IO;

namespace Nitrox.Launcher.Models.Extensions;

public static class StringExtensions
{
    public static string ReplaceInvalidFileNameCharacters(this string value, char replacement = ' ')
    {
        foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidFileNameChar, replacement);
        }
        return value.Trim();
    }
}
