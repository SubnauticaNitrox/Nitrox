using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction;

internal sealed class PathRedactor : IRedactor
{
    public string[] RedactableKeys { get; } = ["path", "filepath"];

    private string GenericUserHomeTag => OperatingSystem.IsWindows() ? "%USERPROFILE%" : "~";

    public RedactResult Redact(ReadOnlySpan<char> value)
    {
        string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (value.StartsWith(userProfilePath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            return RedactResult.Ok($"{GenericUserHomeTag}{value[userProfilePath.Length..]}");
        }
        return RedactResult.Fail();
    }
}
