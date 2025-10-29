using System;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors.Core;

internal interface IRedactor
{
    /// <summary>
    ///     The structured log names that this redactor will try to redact.
    /// </summary>
    string[] RedactableKeys { get; }

    /// <summary>
    ///     Tries to redact the given <paramref name="value" />.
    /// </summary>
    /// <param name="key">The structured log name attached to the value.</param>
    /// <param name="value">The value to redact.</param>
    /// <returns>Redacted value as <see cref="RedactResult.Ok" /> or <see cref="RedactResult.Fail" />.</returns>
    RedactResult Redact(ReadOnlySpan<char> key, ReadOnlySpan<char> value);
}
