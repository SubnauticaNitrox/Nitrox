using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NitroxModel.DataStructures;

[TestClass]
public class NitroxIdTest
{
    private NitroxId id1;
    private NitroxId id2;

    [TestMethod]
    public void SameGuidEquality()
    {
        Guid guid = Guid.NewGuid();
        id1 = new(guid);
        id2 = new(guid);

        (id1 == id2).Should().BeTrue();
        id1.Equals(id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
        (!id1.Equals(id2)).Should().BeFalse();
    }

    [TestMethod]
    public void NullGuidEquality()
    {
        id1 = new();
        id2 = null;

        (id1 == id2).Should().BeFalse();
        id1.Equals(id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
        (!id1.Equals(id2)).Should().BeTrue();
    }

    [TestMethod]
    public void BothNullEquality()
    {
        id1 = id2 = null;
        (id1 != id2).Should().BeFalse();
        (id1 == id2).Should().BeTrue();
    }
}
