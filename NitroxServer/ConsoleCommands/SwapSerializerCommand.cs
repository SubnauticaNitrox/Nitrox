using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal class SwapSerializerCommand : Command
    {
        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;

        public SwapSerializerCommand(ServerConfig serverConfig, WorldPersistence worldPersistence, ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer) : base("swapserializer", Perms.CONSOLE, "Swaps the save format")
        {
            this.worldPersistence = worldPersistence;
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
            this.serverConfig = serverConfig;
        }

        protected override void Execute(CallArgs args)
        {
            serverConfig.SerializerMode = serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? ServerSerializerMode.JSON : ServerSerializerMode.PROTOBUF;
            NitroxConfig.Serialize(serverConfig);

            worldPersistence.UpdateSerializer(serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? protoBufSerializer : jsonSerializer);
            SendMessage(args.Sender, $"Server save format swapped to {serverConfig.SerializerMode}");
        }
    }
}
