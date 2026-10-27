namespace Nitrox.Model.Extensions;

[TestClass]
public class DictionaryExtensionsTest
{
    [TestMethod]
    public void RemoveWhere_WithExtraParameter_ShouldRemoveMatchingItems()
    {
        Dictionary<string, int> dict = new()
        {
            { "a", 1 },
            { "b", 2 },
            { "c", 3 },
            { "d", 4 }
        };
        dict.RemoveWhere(2, (value, parameter) => value % parameter == 0);
        dict.Should().HaveCount(2);
        dict.Should().ContainKey("a").WhoseValue.Should().Be(1);
        dict.Should().ContainKey("c").WhoseValue.Should().Be(3);
    }

    [TestMethod]
    public void RemoveWhere_WithoutParameter_ShouldRemoveMatchingItems()
    {
        Dictionary<string, int> dict = new()
        {
            { "apple", 5 },
            { "banana", 6 },
            { "cherry", 6 }
        };
        dict.RemoveWhere(pair => pair.Key.StartsWith("b") || pair.Value == 5);
        dict.Should().ContainSingle();
        dict.Should().ContainKey("cherry").WhoseValue.Should().Be(6);
    }

    [TestMethod]
    public void RemoveWhere_ShouldHandleEmptyDictionary()
    {
        Dictionary<string, int> dict = [];
        dict.RemoveWhere(pair => true);
        dict.Should().BeEmpty();
    }

    [TestMethod]
    public void RemoveWhere_ShouldHandleRemovingAllItems()
    {
        Dictionary<string, int> dict = new()
        {
            { "a", 1 },
            { "b", 2 }
        };
        dict.RemoveWhere(pair => true);
        dict.Should().BeEmpty();
    }
}
