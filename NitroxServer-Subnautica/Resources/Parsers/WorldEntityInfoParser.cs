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
        Dictionary<string, WorldEntityInfo> worldEntitiesByClassId = new();

        AssetFileInfo assetFileInfo = resourceFile.GetAssetInfo(assetsManager, "WorldEntityData", AssetClassID.MonoBehaviour);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(resourceInst, assetFileInfo);

        foreach (AssetTypeValueField info in assetValue["infos"])
        {
            WorldEntityInfo entityData = new()
            {
                classId = info["classId"].AsString,
                techType = (TechType)info["techType"].AsInt,
                slotType = (EntitySlot.Type)info["slotType"].AsInt,
                prefabZUp = info["prefabZUp"].AsBool,
                localScale = info["localScale"].AsVector3()
            };

            worldEntitiesByClassId.Add(entityData.classId, entityData);
        }

        assetsManager.UnloadAll();
        return worldEntitiesByClassId;
    }
}
