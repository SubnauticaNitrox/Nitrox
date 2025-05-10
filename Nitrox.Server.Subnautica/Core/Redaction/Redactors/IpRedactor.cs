using System;
using System.Net;
using Nitrox.Server.Subnautica.Core.Redaction.Redactors.Core;
using NitroxModel.Helper;

namespace Nitrox.Server.Subnautica.Core.Redaction.Redactors;

internal sealed class IpRedactor : IRedactor
{
    public string[] RedactableKeys { get; } = ["ip", "address", "endpoint"];

    public RedactResult Redact(ReadOnlySpan<char> key, ReadOnlySpan<char> value)
    {
        IPAddress ip;
        ushort port = 0;
        if (value.IndexOf(':') is var separator and >= 0)
        {
            IPAddress.TryParse(value[..separator], out ip);
            ushort.TryParse(value[(separator + 1)..], out port);
        }
        else
        {
            IPAddress.TryParse(value, out ip);
        }

        if (ip != null)
        {
            if (!ip.IsPrivate())
            {
                Span<byte> ipBytes = ip.GetAddressBytes().AsSpan();
                ipBytes.Slice(int.Max(1, ipBytes.Length / 4)).Fill(0);
                ip = new IPAddress(ipBytes);
            }

            if (port > 0)
            {
                return RedactResult.Ok($"{ip}:{port}");
            }
            return RedactResult.Ok(ip.ToString());
        }
        return RedactResult.Fail();
    }
}
