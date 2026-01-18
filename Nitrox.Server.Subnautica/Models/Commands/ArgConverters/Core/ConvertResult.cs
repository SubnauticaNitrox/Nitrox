namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

internal readonly struct ConvertResult
{
    public bool Success { get; init; }
    public object Value { get; init; }

    public static ConvertResult Ok<T>(T value) =>
        new()
        {
            Success = true,
            Value = value
        };

    public static ConvertResult Fail(string message = null) =>
        new() { Success = false, Value = message };
}
