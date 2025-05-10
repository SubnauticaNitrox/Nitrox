using System.Collections.Generic;
using System.IO;
using System.Threading;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using Microsoft.Extensions.Options;
using Nitrox.Server.Subnautica.Models.Resources.Helper;
using NitroxModel.Helper;
using WorldEntityInfo = UWE.WorldEntityInfo;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal class WorldEntitiesResource(SubnauticaAssetsManager assetsManager, IOptions<Configuration.ServerStartOptions> optionsProvider) : IGameResource
{
    private readonly SubnauticaAssetsManager assetsManager = assetsManager;
    private readonly Configuration.ServerStartOptions startOptions = optionsProvider.Value;
    private Task<Dictionary<string, WorldEntityInfo>> worldEntitiesByClassId;
    public Dictionary<string, WorldEntityInfo> WorldEntitiesByClassId => GetWorldEntitiesByClassIdAsync().GetAwaiter().GetResult();

    public Task LoadAsync(CancellationToken cancellationToken)
    {
        worldEntitiesByClassId = GetWorldEntitiesByClassIdAsync(cancellationToken);
        return Task.CompletedTask;
    }

    private async Task<Dictionary<string, WorldEntityInfo>> GetWorldEntitiesByClassIdAsync(CancellationToken cancellationToken = default)
    {
        if (worldEntitiesByClassId != null)
        {
            await worldEntitiesByClassId;
        }

        Dictionary<string, WorldEntityInfo> result = [];

        cancellationToken.ThrowIfCancellationRequested();
        AssetsFileInstance assetFile = assetsManager.LoadAssetsFile(Path.Combine(startOptions.GetSubnauticaResourcesPath(), "resources.assets"), true);
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
}
