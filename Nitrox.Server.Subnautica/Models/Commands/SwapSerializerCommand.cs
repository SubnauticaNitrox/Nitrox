using System.IO;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Serialization;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SwapSerializerCommand : Command
    {
        private readonly Server server;
        private readonly WorldPersistence worldPersistence;
        private readonly SubnauticaServerConfig serverConfig;

        public SwapSerializerCommand(Server server, SubnauticaServerConfig serverConfig, WorldPersistence worldPersistence) : base("swapserializer", Perms.CONSOLE, "Allows to change the save format")
        {
            AddParameter(new TypeEnum<ServerSerializerMode>("serializer", true, "Save format to change to"));

            this.server = server;
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            ServerSerializerMode serializerMode = args.Get<ServerSerializerMode>(0);

            using (serverConfig.Update(Path.Combine(KeyValueStore.Instance.GetSavesFolderDir(), server.Name)))
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
