using System;
using System.Net;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction.Redactors;

internal sealed class IpRedactor : IRedactor
{
    public string[] RedactableKeys { get; } = ["ip", "address", "endpoint"];

    public RedactResult Redact(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        if (!IPEndPoint.TryParse(value, out IPEndPoint endpoint))
        {
            if (IPAddress.TryParse(value, out IPAddress address))
            {
                endpoint = new IPEndPoint(address, 0);
            }
        }
        if (endpoint == null)
        {
            return RedactResult.Fail();
        }

        if (!endpoint.Address.IsLocalhost() && !endpoint.Address.IsPrivate())
        {
            Span<byte> ipBytes = endpoint.Address.GetAddressBytes().AsSpan();
            ipBytes.Slice(int.Max(1, ipBytes.Length / 4)).Fill(0);
            endpoint.Address = new IPAddress(ipBytes);
        }

        if (endpoint.Port > 0)
        {
            return RedactResult.Ok($"{endpoint.Address}:{endpoint.Port}");
        }
        return RedactResult.Ok(endpoint.Address.ToString());
    }
}
