using System.Net;
using Nitrox.Server.Subnautica.Models.Logging.Redaction.Core;

namespace Nitrox.Server.Subnautica.Models.Logging.Redaction;

internal sealed class IpRedactor : IRedactor
{
    public string[] RedactableKeys { get; } = ["ip", "address", "endpoint"];

    public RedactResult Redact(ReadOnlySpan<char> value)
    {
        if (!IPEndPoint.TryParse(value, out IPEndPoint endpoint))
        {
            if (IPAddress.TryParse(value, out IPAddress address))
            {
                endpoint = new IPEndPoint(address, 0);
            }
        }
        if (endpoint is null)
        {
            return RedactResult.Fail();
        }

        bool isTrimmedIp = false;
        if (!endpoint.Address.IsPrivate())
        {
            Span<byte> ipBytes = endpoint.Address.GetAddressBytes().AsSpan();
            ipBytes.Slice(int.Max(1, ipBytes.Length / 4)).Fill(0);
            endpoint.Address = new IPAddress(ipBytes);
            isTrimmedIp = true;
        }

        if (endpoint.Port > 0)
        {
            return RedactResult.Ok($"{endpoint.Address}:{endpoint.Port}{GetPostFix(isTrimmedIp)}");
        }
        return RedactResult.Ok($"{endpoint.Address.ToString()}{GetPostFix(isTrimmedIp)}");
    }

    private string GetPostFix(bool isTrimmedIp) => isTrimmedIp ? " (REDACTED)" : "";
}
