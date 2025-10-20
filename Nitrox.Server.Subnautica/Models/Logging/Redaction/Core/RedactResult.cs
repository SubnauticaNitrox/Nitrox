namespace Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

internal readonly ref struct RedactResult
{
    public ReadOnlySpan<char> Value { get; init; }
    public bool IsRedacted { get; init; }

    public static implicit operator RedactResult(ReadOnlySpan<char> value) => Ok(value);
    public static implicit operator RedactResult(string value) => Ok(value);

    public static RedactResult Ok(ReadOnlySpan<char> value)
    {
        return new RedactResult
        {
            Value = value,
            IsRedacted = true
        };
    }

    public static RedactResult Fail()
    {
        return new RedactResult { IsRedacted = false };
    }
}
