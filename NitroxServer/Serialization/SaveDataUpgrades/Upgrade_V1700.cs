using System;
using Newtonsoft.Json.Linq;
using NitroxServer.Serialization.Upgrade;

namespace NitroxServer.Serialization.SaveDataUpgrades
{
    public class Upgrade_V1700 : SaveDataUpgrade
    {
        public override Version TargetVersion { get; } = new Version(1, 7, 0, 0);

        protected override void UpgradeBaseData(JObject data)
        {
            foreach (JToken piece in data["PartiallyConstructedPieces"])
            {
                if (piece["RotationMetadata"]["value"].Type == JTokenType.Null)
                {
                    continue;
                }

                JToken typeToken = piece["RotationMetadata"]["value"]["$type"];
                switch (typeToken.ToString())
                {
                    case "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.AnchoredFaceRotationMetadata, NitroxModel-Subnautica":
                        typeToken = "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.AnchoredFaceBuilderMetadata, NitroxModel-Subnautica";
                        break;
                    case "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.CorridorRotationMetadata, NitroxModel-Subnautica":
                        typeToken = "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.CorridorBuilderMetadata, NitroxModel-Subnautica";
                        break;
                }
                piece["RotationMetadata"]["value"]["$type"] = typeToken;
            }
            foreach (JToken piece in data["CompletedBasePieceHistory"])
            {
                if (piece["RotationMetadata"]["value"].Type == JTokenType.Null)
                {
                    continue;
                }

                JToken typeToken = piece["RotationMetadata"]["value"]["$type"];
                switch (typeToken.ToString())
                {
                    case "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.AnchoredFaceRotationMetadata, NitroxModel-Subnautica":
                        typeToken = "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.AnchoredFaceBuilderMetadata, NitroxModel-Subnautica";
                        break;
                    case "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.CorridorRotationMetadata, NitroxModel-Subnautica":
                        typeToken = "NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata.CorridorBuilderMetadata, NitroxModel-Subnautica";
                        break;
                }
                piece["RotationMetadata"]["value"]["$type"] = typeToken;
            }
        }

        protected override void UpgradeWorldData(JObject data)
        {
            foreach (JToken inventoryItem in data["InventoryData"]["InventoryItems"])
            {
                if (inventoryItem["$type"] != null && inventoryItem["$type"].Type != JTokenType.Null)
                {
                    // Skip inventory items that already have a '$type' set.
                    continue;
                }

                JProperty newTypeProperty = new("$type", "NitroxModel.DataStructures.GameLogic.BasicItemData, NitroxModel");
                // This ensures that the '$type' property is the first.
                inventoryItem.First.AddBeforeSelf(newTypeProperty);
            }

            foreach (JToken inventoryItem in data["InventoryData"]["StorageSlotItems"])
            {
                if (inventoryItem["$type"] != null && inventoryItem["$type"].Type != JTokenType.Null)
                {
                    // Skip inventory items that already have a '$type' set.
                    continue;
                }

                JProperty newTypeProperty = new("$type", "NitroxModel.DataStructures.GameLogic.BasicItemData, NitroxModel");
                // This ensures that the '$type' property is the first.
                inventoryItem.First.AddBeforeSelf(newTypeProperty);
            }
        }
    }
}
