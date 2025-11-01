using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Nitrox.Server.Subnautica.Models.Serialization.Json;

namespace Nitrox.Server.Subnautica.Models.Serialization.SaveDataUpgrades;

public abstract class SaveDataUpgrade
{
    private static readonly JsonConverter[] converters = [new NitroxIdConverter(), new TechTypeConverter(), new VersionConverter(), new KeyValuePairConverter(), new StringEnumConverter()];

    /// <summary>
    ///     The version the save files will have after the upgrade is done.
    /// </summary>
    public abstract Version TargetVersion { get; }

    public void UpgradeSaveFiles(string saveDir, string fileEnding)
    {
        Log.Info($"┌── Executing {GetType().Name}");
        string baseDataPath = Path.Combine(saveDir, $"BaseData{fileEnding}");
        string playerDataPath = Path.Combine(saveDir, $"PlayerData{fileEnding}");
        string worldDataPath = Path.Combine(saveDir, $"WorldData{fileEnding}");
        string entityDataPath = Path.Combine(saveDir, $"EntityData{fileEnding}");
        string globalRootDataPath = Path.Combine(saveDir, $"GlobalRootData{fileEnding}");

        Log.Info("├── Parsing raw json");
        JObject baseData = TryParseFile(baseDataPath);
        JObject playerData = TryParseFile(playerDataPath);
        JObject worldData = TryParseFile(worldDataPath);
        JObject entityData = TryParseFile(entityDataPath);
        JObject globalRootData = TryParseFile(globalRootDataPath);

        Log.Info("├── Applying upgrade scripts");
        if (baseData != null)
        {
            UpgradeBaseData(baseData);
        }
        if (playerData != null)
        {
            UpgradePlayerData(playerData);
        }
        if (worldData != null)
        {
            UpgradeWorldData(worldData);
        }
        if (entityData != null)
        {
            UpgradeEntityData(entityData);
        }
        if (globalRootData != null)
        {
            UpgradeGlobalRootData(globalRootData);
        }

        Log.Info("└── Saving to disk");
        WriteJObjectIfNotNull(baseDataPath, baseData);
        WriteJObjectIfNotNull(playerDataPath, playerData);
        WriteJObjectIfNotNull(worldDataPath, worldData);
        WriteJObjectIfNotNull(entityDataPath, entityData);
        WriteJObjectIfNotNull(globalRootDataPath, globalRootData);
    }

    protected virtual void UpgradeBaseData(JObject data) { }
    protected virtual void UpgradePlayerData(JObject data) { }
    protected virtual void UpgradeWorldData(JObject data) { }
    protected virtual void UpgradeEntityData(JObject data) { }
    protected virtual void UpgradeGlobalRootData(JObject data) { }

    private static JObject? TryParseFile(string filePath)
    {
        try
        {
            return JObject.Parse(File.ReadAllText(filePath));
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static void WriteJObjectIfNotNull(string filePath, JObject? obj)
    {
        string content = obj?.ToString(Formatting.None, converters);
        if (content == null)
        {
            return;
        }
        File.WriteAllText(filePath, content);
    }
}
