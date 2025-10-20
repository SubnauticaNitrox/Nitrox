using System;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors;

internal sealed class PathRedactor : IRedactor
{
    public string[] RedactableKeys { get; } = ["path", "filepath"];

    public RedactResult Redact(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (value.StartsWith(userProfilePath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
        {
            if (OperatingSystem.IsWindows())
            {
                return RedactResult.Ok($"%USERPROFILE%{value[userProfilePath.Length..]}");
            }
            return RedactResult.Ok($"~{value[userProfilePath.Length..]}");
        }
        return RedactResult.Fail();
    }
}
