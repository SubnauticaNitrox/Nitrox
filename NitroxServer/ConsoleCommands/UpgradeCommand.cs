using System;
using System.IO;
using System.Threading;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;
using NitroxServer.Serialization.Upgrade;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal sealed class UpgradeCommand : Command
    {
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;
        private readonly SaveDataUpgrade[] upgrades;
        private readonly Server server;

        public UpgradeCommand(WorldPersistence worldPersistence, ServerConfig serverConfig, SaveDataUpgrade[] upgrades, Server server) : base("upgrade", Perms.CONSOLE, "Upgrades the save file to the next version")
        {
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.upgrades = upgrades;
            this.server = server;
        }

        protected override void Execute(CallArgs args)
        {
            string saveDir = serverConfig.SaveName;
            string fileEnding = worldPersistence.SaveDataSerializer.GetFileEnding();
            SaveFileVersion saveFileVersion = worldPersistence.SaveDataSerializer.Deserialize<SaveFileVersion>(Path.Combine(saveDir, "Version" + fileEnding));


            if (saveFileVersion.Version == NitroxEnvironment.Version)
            {
                SendMessage(args.Sender, "Save files are already at the newest version");
            }
            else if (serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF)
            {
                SendMessage(args.Sender, "Can't upgrade while using ProtoBuf as serializer");
            }
            else
            {
                foreach (SaveDataUpgrade upgrade in upgrades)
                {
                    if (upgrade.TargetVersion > saveFileVersion.Version)
                    {
                        SendMessage(args.Sender, $"Applying {upgrade.GetType().Name}");
                        upgrade.UpgradeData(saveDir, fileEnding);
                    }
                }
                worldPersistence.SaveDataSerializer.Serialize(Path.Combine(saveDir, "Version" + fileEnding), new SaveFileVersion());
                SendMessage(args.Sender, $"Save file was successfully upgraded to {NitroxEnvironment.Version}");
                server.Stop(false);
                Thread.Sleep(2000);
            }
        }
    }
}
