using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class SwapSerializerCommand : Command
    {
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;

        public SwapSerializerCommand(ServerConfig serverConfig, WorldPersistence worldPersistence) : base("swapserializer", Perms.CONSOLE, "Allows to change the save format")
        {
            AddParameter(new TypeEnum<ServerSerializerMode>("serializer", true));

            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            ServerSerializerMode serializerMode = args.Get<ServerSerializerMode>(0);

            if (serializerMode != serverConfig.SerializerMode)
            {
                serverConfig.SerializerMode = serializerMode;
                NitroxConfig.Serialize(serverConfig);

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
