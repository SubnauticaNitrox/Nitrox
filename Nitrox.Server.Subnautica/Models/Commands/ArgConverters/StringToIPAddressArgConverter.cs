using System.Net;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a string to an <see cref="IPAddress" />, if it is a valid IPv4/IPv6 literal.
/// </summary>
internal sealed class StringToIPAddressArgConverter : IArgConverter<string, IPAddress>
{
    public Task<ConvertResult> ConvertAsync(string value)
    {
        if (!IPAddress.TryParse(value, out IPAddress? address))
        {
            return Task.FromResult(ConvertResult.Fail($"'{value}' is not a valid IP address"));
        }
        if (int.TryParse(value, out _))
        {
            return Task.FromResult(ConvertResult.Fail($"'{value}' must not be an integer to be parsed as an IP address"));
        }
        return Task.FromResult(ConvertResult.Ok(address));
    }
}
