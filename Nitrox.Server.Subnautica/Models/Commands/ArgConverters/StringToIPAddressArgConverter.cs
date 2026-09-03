using System.Net;
using Nitrox.Server.Subnautica.Models.Commands.ArgConverters.Core;

namespace Nitrox.Server.Subnautica.Models.Commands.ArgConverters;

/// <summary>
///     Converts a string to an <see cref="IPAddress" />, if it is a valid IPv4/IPv6 literal.
/// </summary>
internal sealed class StringToIPAddressArgConverter : IArgConverter<string, IPAddress>
{
    public Task<ConvertResult> ConvertAsync(string value) =>
        Task.FromResult(IPAddress.TryParse(value, out IPAddress address)
                            ? ConvertResult.Ok(address)
                            : ConvertResult.Fail($"'{value}' is not a valid IP address"));
}
