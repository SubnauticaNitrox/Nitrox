using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures;

[TestClass]
public class NitroxVersionTest
{
    [TestMethod]
    public void Equals()
    {
        NitroxVersion a = new(2, 1);
        NitroxVersion b = new(1, 15);

        NitroxVersion source = new(2, 1);
        source.Equals(a).Should().BeTrue();
        source.Equals(b).Should().BeFalse();
    }

    [TestMethod]
    public void Compare()
    {
        NitroxVersion source = new(2, 1);
        source.CompareTo(new(2, 1)).Should().Be(0);
        source.CompareTo(new(1, 15)).Should().Be(1);
        source.CompareTo(new (2, 2)).Should().Be(-1);
        source.CompareTo(new (3, 1)).Should().Be(-1);
    }
}
