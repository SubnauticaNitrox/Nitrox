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
            var a = info["classId"];
            var aa = info["classId"].AsString;
            var b = info["techType"];
            var bb = info["techType"].AsInt;
            var c = info["slotType"];
            var cc = info["slotType"].AsInt;
            var d = info["prefabZUp"];
            var dd = info["prefabZUp"].AsBool;
            var e = info["localScale"];
            var ee = info["localScale"].AsVector3();
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
