using System.Net;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.Helper;

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
}
