using System.Text;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Test.Server.Resources;

[TestClass]
public class PrefabAddressableCatalogTest
{
    [TestMethod]
    public void ReadPrefabDatabase_ReadsClassIdsAndPrefabPaths()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, true))
        {
            writer.Write(2);
            writer.Write("class-id-one");
            writer.Write("Assets/Prefabs/one.prefab");
            writer.Write("class-id-two");
            writer.Write("Assets/Prefabs/two.prefab");
        }
        stream.Position = 0;

        Dictionary<string, string> result = PrefabAddressableCatalog.ReadPrefabDatabase(stream);

        result.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            ["class-id-one"] = "Assets/Prefabs/one.prefab",
            ["class-id-two"] = "Assets/Prefabs/two.prefab"
        });
    }

    [TestMethod]
    public void ReadPrefabDatabase_DuplicateClassId_UsesLatestPrefabPath()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream, Encoding.UTF8, true))
        {
            writer.Write(2);
            writer.Write("class-id");
            writer.Write("Assets/Prefabs/old.prefab");
            writer.Write("class-id");
            writer.Write("Assets/Prefabs/new.prefab");
        }
        stream.Position = 0;

        Dictionary<string, string> result = PrefabAddressableCatalog.ReadPrefabDatabase(stream);

        result.Should().ContainSingle();
        result["class-id"].Should().Be("Assets/Prefabs/new.prefab");
    }
}
