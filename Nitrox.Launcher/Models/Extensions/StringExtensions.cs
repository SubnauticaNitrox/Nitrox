using System.IO;

namespace Nitrox.Launcher.Models.Extensions;

public static class StringExtensions
{
    public static string ReplaceInvalidFileNameCharacters(this string value)
    {
        foreach (char invalidFileNameChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidFileNameChar, ' ');
        }
        return value.Trim();
    }
}
