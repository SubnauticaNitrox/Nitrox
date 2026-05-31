using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a word to a bool value.
/// </summary>
internal sealed class WordToBoolArgConverter : IArgConverter<string, bool>
{
    public Task<ConvertResult> ConvertAsync(string value) =>
        Task.FromResult(value switch
        {
            "on" or "enable" or "1" => ConvertResult.Ok(true),
            "off" or "disable" or "0" => ConvertResult.Ok(false),
            _ => ConvertResult.Fail()
        });
}
