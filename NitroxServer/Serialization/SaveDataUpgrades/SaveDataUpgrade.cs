using System;
using System.IO;

namespace NitroxServer.Serialization.Upgrade
{
    public abstract class SaveDataUpgrade
    {
        public abstract Version TargetVersion { get; }

        public void UpgradeData(string saveDir, string fileEnding)
        {
            string baseDataPath = Path.Combine(saveDir, "BaseData" + fileEnding);
            string playerDataPath = Path.Combine(saveDir, "PlayerData" + fileEnding);
            string worldDataPath = Path.Combine(saveDir, "WorldData" + fileEnding);
            string entityDataPath = Path.Combine(saveDir, "EntityData" + fileEnding);

            File.WriteAllText(baseDataPath, UpgradeBaseData(File.ReadAllText(baseDataPath)));
            File.WriteAllText(playerDataPath, UpgradePlayerData(File.ReadAllText(playerDataPath)));
            File.WriteAllText(worldDataPath, UpgradeWorldData(File.ReadAllText(worldDataPath)));
            File.WriteAllText(entityDataPath, UpgradeEntityData(File.ReadAllText(entityDataPath)));
        }

        protected virtual string UpgradeBaseData(string data)
        {
            return data;
        }

        protected virtual string UpgradePlayerData(string data)
        {
            return data;
        }

        protected virtual string UpgradeWorldData(string data)
        {
            return data;
        }

        protected virtual string UpgradeEntityData(string data)
        {
            return data;
        }

    }
}
