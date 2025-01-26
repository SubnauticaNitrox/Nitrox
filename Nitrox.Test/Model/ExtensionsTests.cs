namespace NitroxModel;

[TestClass]
public class ExtensionsTest
{
    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenEmptyList()
    {
        object[] list = [];
        list.Should().BeEmpty();
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_ShouldDoNothingWhenAlwaysFalsePredicate()
    {
        List<string> list = ["one", "two", "three", "four"];
        list.RemoveAllFast((object)null, static (_, _) => false);
        list.Should().BeEquivalentTo("one", "two", "three", "four");
    }

    [TestMethod]
    public void RemoveAllFast_ThrowsErrorIfFixedSizeList()
    {
        string[] list = ["one", "two", "three"];
        Assert.ThrowsException<NotSupportedException>(() => list.RemoveAllFast((object)null, static (item, _) => item == "one"));
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveFirstItem()
    {
        List<string> list = ["one", "two", "three"];
        list.RemoveAllFast((object)null, static (item, _) => item == "one");
        list.Should().BeEquivalentTo("two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveMidItems()
    {
        List<string> list = ["one", "two", "three", "four"];
        list.RemoveAllFast((object)null, static (item, _) => item == "two" || item == "three");
        list.Should().BeEquivalentTo("one", "four");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveEndItem()
    {
        List<string> list = ["one", "two", "three", "four"];
        list.RemoveAllFast((object)null, static (item, _) => item == "four");
        list.Should().BeEquivalentTo("one", "two", "three");
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveAllItems()
    {
        List<string> list = ["one", "two", "three", "four"];
        list.RemoveAllFast((object)null, static (_, _) => true);
        list.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveAllFast_CanRemoveItemsWithExtraParameterInPredicate()
    {
        List<string> list = ["one", "two", "three", "four"];
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

    [TestMethod]
    public void GetCommandArgs()
    {
        Array.Empty<string>().GetCommandArgs("").Should().BeEmpty();
        Array.Empty<string>().GetCommandArgs("--something").Should().BeEmpty();
        new[] { "/bin/nitrox.dll", "--save", "My World" }.GetCommandArgs("--save").Should().BeEquivalentTo("My World");
        new[] { "--nitrox", @"C:\a\path" }.GetCommandArgs("--nitrox").Should().BeEquivalentTo(@"C:\a\path");
        new[] { "blabla", "--other=test", "--nitrox", @"C:\a\path" }.GetCommandArgs("--nitrox").Should().BeEquivalentTo(@"C:\a\path");
        new[] { "blabla", "--other=test", "--nitrox", @"C:\a\path" }.GetCommandArgs("--other").Should().BeEquivalentTo("test");
        new[] { "blabla", "--other=test", "other2", "--nitrox", @"C:\a\path" }.GetCommandArgs("--other").Should().BeEquivalentTo("test");
        new[] { "blabla", "--other", "test", "other2", "--nitrox", @"C:\a\path" }.GetCommandArgs("--other").Should().BeEquivalentTo("test", "other2");
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
