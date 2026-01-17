using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction;

internal sealed class PasswordRedactor : IRedactor
{
    /// <summary>
    ///     Small passwords will be redacted as this many stars to obfuscate security weaknesses.
    /// </summary>
    private const int PASSWORD_PAD_LENGTH = 8;

    public string[] RedactableKeys { get; } = ["password"];

    public RedactResult Redact(ReadOnlySpan<char> value)
    {
        if (value.Length < 1)
        {
            return ReadOnlySpan<char>.Empty;
        }
        return new string('*', int.Max(PASSWORD_PAD_LENGTH, value.Length));
    }
}
