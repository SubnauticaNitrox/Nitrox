namespace Nitrox.Model.DataStructures;

[TestClass]
public class NitroxIdTest
{
    [TestMethod]
    public void SameGuidEquality()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        NitroxId id1 = new(guid);
        NitroxId id2 = new(guid);

        // Act & Assert
        (id1 == id2).Should().BeTrue();
        id1.Equals(id2).Should().BeTrue();
        (id1 != id2).Should().BeFalse();
        (!id1.Equals(id2)).Should().BeFalse();
    }

    [TestMethod]
    public void SameReferenceEquality()
    {
        // Arrange
        NitroxId id1 = new(Guid.NewGuid());
        NitroxId id2 = id1;

        // Act & Assert
        (id1 == id2).Should().BeTrue();
        id1.Equals(id2).Should().BeTrue();
        id1.Equals((object)id2).Should().BeTrue();
    }

    [TestMethod]
    public void DifferentGuidEquality()
    {
        // Arrange
        NitroxId id1 = new(Guid.NewGuid());
        NitroxId id2 = new(Guid.NewGuid());

        // Act & Assert
        (id1 == id2).Should().BeFalse();
        id1.Equals(id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
    }

    [TestMethod]
    public void NullGuidEquality()
    {
        // Arrange
        NitroxId id1 = new();
        NitroxId id2 = null;

        // Act & Assert
        (id1 == id2).Should().BeFalse();
        id1.Equals(id2).Should().BeFalse();
        (id1 != id2).Should().BeTrue();
        (!id1.Equals(id2)).Should().BeTrue();
    }

    [TestMethod]
    public void BothNullEquality()
    {
        // Arrange
        NitroxId? id1 = null;
        NitroxId? id2 = null;

        // Act & Assert
        (id1 != id2).Should().BeFalse();
        (id1 == id2).Should().BeTrue();
    }

    [TestMethod]
    public void EqualsObjectReturnsFalseForDifferentType()
    {
        // Arrange
        NitroxId id = new(Guid.NewGuid());

        // Act & Assert
        id.Equals(new object()).Should().BeFalse();
    }

    [TestMethod]
    public void EqualIdsHaveSameHashCode()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        NitroxId id1 = new(guid);
        NitroxId id2 = new(guid);

        // Act & Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [TestMethod]
    public void CompareToReturnsZeroForEqualIds()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        NitroxId id1 = new(guid);
        NitroxId id2 = new(guid);

        // Act & Assert
        id1.CompareTo(id2).Should().Be(0);
    }

    [TestMethod]
    public void CompareToReturnsOneForNull()
    {
        // Arrange
        NitroxId id = new(Guid.NewGuid());

        // Act & Assert
        id.CompareTo(null).Should().Be(1);
    }

    [TestMethod]
    public void CompareToUsesUnderlyingGuidOrdering()
    {
        // Arrange
        NitroxId id1 = new("00000000-0000-0000-0000-000000000001");
        NitroxId id2 = new("00000000-0000-0000-0000-000000000002");

        // Act & Assert
        id1.CompareTo(id2).Should().BeNegative();
        id2.CompareTo(id1).Should().BePositive();
    }

    [TestMethod]
    public void IncrementAdvancesId()
    {
        // Arrange
        NitroxId id = new(Guid.Empty);

        // Act
        NitroxId next = id.Increment();

        // Assert
        next.Should().NotBe(id);
        next.CompareTo(id).Should().BePositive();
    }

    [TestMethod]
    public void IncrementWrapsAroundFromMaxValue()
    {
        // Arrange
        NitroxId id = new(new Guid(
        [
            255, 255, 255, 255,
            255, 255, 255, 255,
            255, 255, 255, 255,
            255, 255, 255, 255
        ]));

        // Act
        NitroxId wrapped = id.Increment();

        // Assert
        wrapped.Should().Be(new NitroxId(Guid.Empty));
    }
}
