namespace Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

internal interface IRedactor
{
    /// <summary>
    ///     The structured log names that this redactor will try to redact. This is case-insensitive.
    /// </summary>
    string[] RedactableKeys { get; }

    /// <summary>
    ///     Tries to redact the given <paramref name="value" />.
    /// </summary>
    /// <param name="value">The value to redact.</param>
    /// <returns>Redacted value as <see cref="RedactResult.Ok" /> or <see cref="RedactResult.Fail" />.</returns>
    RedactResult Redact(ReadOnlySpan<char> value);
}
