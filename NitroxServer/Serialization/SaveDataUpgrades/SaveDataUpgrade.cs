using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NitroxModel.DataStructures.JsonConverter;

namespace NitroxServer.Serialization.Upgrade
{
    public abstract class SaveDataUpgrade
    {
        private readonly JsonConverter[] converters = { new NitroxIdConverter(), new TechTypeConverter(), new VersionConverter(), new KeyValuePairConverter(), new StringEnumConverter() };

        public abstract Version TargetVersion { get; }

        public void UpgradeSaveFiles(string saveDir, string fileEnding)
        {
            Log.Info($"┌── Executing {GetType().Name}");
            string baseDataPath = Path.Combine(saveDir, "BaseData" + fileEnding);
            string playerDataPath = Path.Combine(saveDir, "PlayerData" + fileEnding);
            string worldDataPath = Path.Combine(saveDir, "WorldData" + fileEnding);
            string entityDataPath = Path.Combine(saveDir, "EntityData" + fileEnding);

            Log.Info("├── Parsing raw json");
            JObject baseData = JObject.Parse(File.ReadAllText(baseDataPath));
            JObject playerData = JObject.Parse(File.ReadAllText(playerDataPath));
            JObject worldData = JObject.Parse(File.ReadAllText(worldDataPath));
            JObject entityData = JObject.Parse(File.ReadAllText(entityDataPath));

            Log.Info("├── Applying upgrade scripts");
            UpgradeBaseData(baseData);
            UpgradePlayerData(playerData);
            UpgradeWorldData(worldData);
            UpgradeEntityData(entityData);

            Log.Info("└── Saving to disk");
            File.WriteAllText(baseDataPath, baseData.ToString(Formatting.None, converters));
            File.WriteAllText(playerDataPath, playerData.ToString(Formatting.None, converters));
            File.WriteAllText(worldDataPath, worldData.ToString(Formatting.None, converters));
            File.WriteAllText(entityDataPath, entityData.ToString(Formatting.None, converters));
        }

        protected virtual void UpgradeBaseData(JObject data) { }
        protected virtual void UpgradePlayerData(JObject data) { }
        protected virtual void UpgradeWorldData(JObject data) { }
        protected virtual void UpgradeEntityData(JObject data) { }
    }
}
