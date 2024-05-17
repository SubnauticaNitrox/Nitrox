using System.Collections.Generic;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxServer_Subnautica.Resources.Parsers.Abstract;
using NitroxServer_Subnautica.Resources.Parsers.Helper;
using UWE;

namespace NitroxServer_Subnautica.Resources.Parsers;

public class WorldEntityInfoParser : ResourceFileParser<Dictionary<string, WorldEntityInfo>>
{
    public override Dictionary<string, WorldEntityInfo> ParseFile()
    {
        Dictionary<string, WorldEntityInfo> worldEntitiesByClassId = [];

        AssetFileInfo assetFileInfo = resourceFile.GetAssetInfo(assetsManager, "WorldEntityData", AssetClassID.MonoBehaviour);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(resourceInst, assetFileInfo);

        AssetTypeValueField worldEntityArray = assetValue["infos"]["Array"];

        foreach (AssetTypeValueField info in worldEntityArray)
        {
            WorldEntityInfo entityData = new()
            {
                classId = info["classId"].AsString,
                techType = (TechType)info["techType"].AsInt,
                slotType = (EntitySlot.Type)info["slotType"].AsInt,
                prefabZUp = info["prefabZUp"].AsBool,
                cellLevel = (LargeWorldEntity.CellLevel)info["cellLevel"].AsInt,
                localScale = info["localScale"].ToVector3()
            };

            worldEntitiesByClassId.Add(entityData.classId, entityData);
        }

        return worldEntitiesByClassId;
    }
}
