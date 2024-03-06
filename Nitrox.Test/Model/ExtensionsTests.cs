namespace NitroxModel;

[TestClass]
public class ExtensionsTest
{
    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenEmptyList()
    {
        object[] list = Array.Empty<object>();
        list.Should().BeEmpty();
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenAlwaysFalsePredicate()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (_, _) => false);
        list.Should().BeEquivalentTo("one", "two", "three", "four");
    }

    [TestMethod]
    public void RemoveAllFast_ThrowsErrorIfFixedSizeList()
    {
        string[] list = { "one", "two", "three" };
        Assert.ThrowsException<NotSupportedException>(() => list.RemoveAllFast((object)null, static (item, _) => item == "one"));
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveFirstItem()
    {
        List<string> list = new() { "one", "two", "three" };
        list.RemoveAllFast((object)null, static (item, _) => item == "one");
        list.Should().BeEquivalentTo("two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveMidItems()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (item, _) => item == "two" || item == "three");
        list.Should().BeEquivalentTo("one", "four");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveEndItem()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (item, _) => item == "four");
        list.Should().BeEquivalentTo("one", "two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveAllItems()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveItemsWithExtraParameterInPredicate()
    {
        List<string> list = new() { "one", "two", "three", "four" };
        list.RemoveAllFast(3, static (item, length) => item.Length == length);
        list.Should().BeEquivalentTo("three", "four");
    }

    [TestMethod]
    public void GetUniqueNonCombinatoryFlags_ShouldReturnUniqueNonCombinatoryFlags()
    {
        TestEnumFlags.ALL.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
        TestEnumFlags.CDEF.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
        TestEnumFlags.E.GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.E]);
        TestEnumFlags.NONE.GetUniqueNonCombinatoryFlags().Should().BeEmpty();
    }

    [TestMethod]
    public void GetUniqueNonCombinatorFlags_ShouldReturnAllUniquesWhenAllBitsSet()
    {
        ((TestEnumFlags)int.MaxValue).GetUniqueNonCombinatoryFlags().Should().BeEquivalentTo([TestEnumFlags.A, TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E, TestEnumFlags.F]);
    }

    [Flags]
    private enum TestEnumFlags
    {
        NONE = 0,
        F = 1 << 5,
        A = 1 << 0,
        B = 1 << 1,
        C = 1 << 2,
        D = 1 << 3,
        E = 1 << 4,
        AB = A | B,
        CD = C | D,
        CDEF = CD | E | F,
        ALL = AB | CDEF
    }
}
