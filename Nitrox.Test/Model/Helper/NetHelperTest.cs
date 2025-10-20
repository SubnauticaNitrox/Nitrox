﻿using System.Net;
using System.Net.Sockets;

namespace Nitrox.Model.Helper;

[TestClass]
public class NetHelperTest
{
    [TestMethod]
    public void ShouldMatchPrivateIps()
    {
        // Tested subnet ranges that are reserved for private networks:
        // 10.0.0.0/8
        // 127.0.0.0/8
        // 172.16.0.0/12
        // 192.0.0.0/24
        // 192.168.0.0/16
        // 198.18.0.0/15

        IPAddress.Parse("10.0.0.0").IsPrivate().Should().BeTrue();
        IPAddress.Parse("10.0.0.255").IsPrivate().Should().BeTrue();
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
    }

    [TestMethod]
    public void ShouldMatchLocalhostIps()
    {
        IPAddress GetSlightlyDifferentIp(IPAddress address)
        {
            if (address.AddressFamily != AddressFamily.InterNetwork)
            {
                throw new Exception("Only supports IPv4");
            }
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

        IPAddress.Parse("127.0.0.1").IsLocalhost().Should().BeTrue();
        IPAddress.Parse("127.0.0.2").IsLocalhost().Should().BeTrue();
        IPAddress.Parse("192.168.0.255").IsLocalhost().Should().BeFalse();
        NetHelper.GetLanIp().IsLocalhost().Should().BeTrue();
        IPAddress differentIp = GetSlightlyDifferentIp(NetHelper.GetLanIp());
        differentIp.Should().NotBeEquivalentTo(NetHelper.GetLanIp());
        differentIp.IsLocalhost().Should().BeFalse();
    }
}
