using System.Net;
using Nitrox.Model.Helper;

namespace Nitrox.Model.Extensions;

[TestClass]
public class IpAddressExtensionsTest
{
    [TestMethod]
    public void ShouldMatchPrivateIps()
    {
        // IPv4
        IPAddress.Parse("10.0.0.0").IsPrivate().Should().BeTrue();
        IPAddress.Parse("10.0.0.255").IsPrivate().Should().BeTrue();
        IPAddress.Parse("127.0.0.1").IsPrivate().Should().BeTrue();
        IPAddress.Parse("172.31.255.255").IsPrivate().Should().BeTrue();
        IPAddress.Parse("172.31.255.255").IsPrivate().Should().BeTrue();
        IPAddress.Parse("192.0.0.255").IsPrivate().Should().BeTrue();
        IPAddress.Parse("192.168.2.1").IsPrivate().Should().BeTrue();
        IPAddress.Parse("192.168.2.254").IsPrivate().Should().BeTrue();
        IPAddress.Parse("192.168.2.255").IsPrivate().Should().BeTrue();
        IPAddress.Parse("198.18.0.1").IsPrivate().Should().BeTrue();
        IPAddress.Parse("198.19.255.255").IsPrivate().Should().BeTrue();

        IPAddress.Parse("9.255.255.255").IsPrivate().Should().BeFalse();
        IPAddress.Parse("91.63.176.12").IsPrivate().Should().BeFalse();
        IPAddress.Parse("172.32.0.1").IsPrivate().Should().BeFalse();
        IPAddress.Parse("192.0.1.0").IsPrivate().Should().BeFalse();
        IPAddress.Parse("198.17.255.255").IsPrivate().Should().BeFalse();
        IPAddress.Parse("198.20.0.0").IsPrivate().Should().BeFalse();

        // IPv6
        IPAddress.Parse("::1").IsPrivate().Should().BeTrue();
        IPAddress.Parse("fc00:ff00::").IsPrivate().Should().BeTrue();
        IPAddress.Parse("fe80::").IsPrivate().Should().BeTrue();
        IPAddress.Parse("fe80::ffff:ffff:ffff:ffff").IsPrivate().Should().BeTrue();
        IPAddress.Parse("fe80:0000:0000:0000:0000:0000:0000:0001").IsPrivate().Should().BeTrue();
        IPAddress.Parse("febf:ffff:ffff:ffff:ffff:ffff:ffff:fffe").IsPrivate().Should().BeTrue();

        IPAddress.Parse("fec0::").IsPrivate().Should().BeFalse();
        IPAddress.Parse("fecf::").IsPrivate().Should().BeFalse();
        IPAddress.Parse("fe7f::").IsPrivate().Should().BeFalse();
    }

    [TestMethod]
    public void ShouldMatchLocalhostIps()
    {
        IPAddress.Parse("127.0.0.1").IsLocalhost().Should().BeTrue();
        IPAddress.Parse("127.0.0.2").IsLocalhost().Should().BeTrue();
        IPAddress.Parse("192.168.0.255").IsLocalhost().Should().BeFalse();
        NetHelper.GetLanUsableIp().IsLocalhost().Should().BeTrue();
        IPAddress differentIp = GetSlightlyDifferentIp(NetHelper.GetLanUsableIp()!);
        differentIp.Should().NotBeEquivalentTo(NetHelper.GetLanUsableIp());
        differentIp.IsLocalhost().Should().BeFalse();

        IPAddress GetSlightlyDifferentIp(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            unchecked
            {
                while (bytes[3] is < 1 or > 253)
                {
                    bytes[3]++;
                }
                bytes[3]++;
            }
            return new IPAddress(bytes);
        }
    }
}
