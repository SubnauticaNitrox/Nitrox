﻿using System.IO;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class SwapSerializerCommand : Command
    {
        private readonly WorldPersistence worldPersistence;
        private readonly SubnauticaServerConfig serverConfig;

        public SwapSerializerCommand(SubnauticaServerConfig serverConfig, WorldPersistence worldPersistence) : base("swapserializer", Perms.CONSOLE, "Allows to change the save format")
        {
            AddParameter(new TypeEnum<ServerSerializerMode>("serializer", true, "Save format to change to"));

            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            ServerSerializerMode serializerMode = args.Get<ServerSerializerMode>(0);

            using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), serverConfig.SaveName)))
            {
                if (serializerMode != serverConfig.SerializerMode)
                {
                    serverConfig.SerializerMode = serializerMode;
                    worldPersistence.UpdateSerializer(serializerMode);
                    SendMessage(args.Sender, $"Server save format swapped to {serverConfig.SerializerMode}");
                }
                else
                {
                    SendMessage(args.Sender, "Server is already using this save format");
                }
            }
        }
    }
}
