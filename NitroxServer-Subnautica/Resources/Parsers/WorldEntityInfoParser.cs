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
           foreach (AssetTypeValueField entityInfo in info)
           {
               WorldEntityInfo entityData = new()
               {
                   classId = entityInfo["classId"].AsString,
                   techType = (TechType)entityInfo["techType"].AsInt,
                   slotType = (EntitySlot.Type)entityInfo["slotType"].AsInt,
                   prefabZUp = entityInfo["prefabZUp"].AsBool,
                   cellLevel = (LargeWorldEntity.CellLevel)entityInfo["cellLevel"].AsInt,
                   localScale = entityInfo["localScale"].ToVector3()
               };

               worldEntitiesByClassId.Add(entityData.classId, entityData);

            }
        }

        assetsManager.UnloadAll();
        return worldEntitiesByClassId;
    }
}
