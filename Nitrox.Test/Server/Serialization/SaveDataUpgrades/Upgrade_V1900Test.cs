using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Nitrox.Model.Constants;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;

[TestClass]
public sealed class Upgrade_V1900Test
{
    [TestMethod]
    public void UpgradeSaveFiles_WrapsMetadataObjectsInArrays_AtAnyNestingDepth()
    {
        string saveDir = Path.Combine(Path.GetTempPath(), $"NitroxTest_{nameof(Upgrade_V1900Test)}_{Path.GetRandomFileName()}");
        Directory.CreateDirectory(saveDir);
        try
        {
            string entityDataPath = Path.Combine(saveDir, $"EntityData{NitroxConstants.SAVE_FILE_ENDING}");
            File.WriteAllText(entityDataPath, /* lang=json */ """
                {
                  "Entities": [
                    {
                      "Id": "top-level",
                      "Metadata": { "Health": 50.0 },
                      "ChildEntities": [
                        {
                          "Id": "nested-child",
                          "Metadata": { "Charge": 1.0 },
                          "ChildEntities": []
                        }
                      ]
                    },
                    {
                      "Id": "no-metadata-yet",
                      "Metadata": null,
                      "ChildEntities": []
                    }
                  ]
                }
                """);

            Upgrade_V1900 upgrade = new(Substitute.For<ILogger<Upgrade_V1900>>());
            upgrade.UpgradeSaveFiles(saveDir, NitroxConstants.SAVE_FILE_ENDING);

            JArray entities = (JArray)JObject.Parse(File.ReadAllText(entityDataPath))["Entities"];

            JToken topLevelMetadata = entities[0]["Metadata"];
            Assert.AreEqual(JTokenType.Array, topLevelMetadata.Type);
            Assert.AreEqual(50.0, (double)topLevelMetadata[0]["Health"]);

            JToken nestedMetadata = entities[0]["ChildEntities"][0]["Metadata"];
            Assert.AreEqual(JTokenType.Array, nestedMetadata.Type);
            Assert.AreEqual(1.0, (double)nestedMetadata[0]["Charge"]);

            Assert.AreEqual(JTokenType.Null, entities[1]["Metadata"].Type);
        }
        finally
        {
            Directory.Delete(saveDir, true);
        }
    }
}
