using System.Collections.Generic;
using System.IO;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Nitrox.Server.Subnautica.Models.Resources.Core;
using WorldEntityInfo = UWE.WorldEntityInfo;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal static class WorldEntityDataParser
{
    public static Dictionary<string, WorldEntityInfo> Load(
        SubnauticaAssetsManager assetsManager,
        ServerStartOptions options,
        CancellationToken cancellationToken = default)
    {
        Dictionary<string, WorldEntityInfo> result = [];

        cancellationToken.ThrowIfCancellationRequested();
        AssetsFileInstance assetFile = assetsManager.LoadAssetsFile(GetResourcesPath(options), true);
        AssetFileInfo assetFileInfo = assetFile.file.GetAssetInfo(assetsManager, "WorldEntityData", AssetClassID.MonoBehaviour);
        AssetTypeValueField assetValue = assetsManager.GetBaseField(assetFile, assetFileInfo);

        foreach (AssetTypeValueField info in assetValue["infos"])
        {
            cancellationToken.ThrowIfCancellationRequested();
            WorldEntityInfo entityData = new()
            {
                classId = info["classId"].AsString,
                techType = (TechType)info["techType"].AsInt,
                slotType = (EntitySlot.Type)info["slotType"].AsInt,
                prefabZUp = info["prefabZUp"].AsBool,
                cellLevel = (LargeWorldEntity.CellLevel)info["cellLevel"].AsInt,
                localScale = info["localScale"].ToVector3()
            };

            result.Add(entityData.classId, entityData);
        }

        Validate.IsTrue(result.Count > 0);
        return result;
    }

    public static string GetSourceFingerprint(ServerStartOptions options)
    {
        FileInfo file = new(GetResourcesPath(options));
        return $"{file.Length:x}:{file.LastWriteTimeUtc.Ticks:x}";
    }

    private static string GetResourcesPath(ServerStartOptions options) => Path.Combine(options.GetSubnauticaResourcesPath(), "resources.assets");
}
