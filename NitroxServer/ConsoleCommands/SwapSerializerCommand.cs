using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal sealed class SwapSerializerCommand : Command
    {
        private readonly WorldPersistence worldPersistence;
        private readonly ServerConfig serverConfig;
        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;

        public SwapSerializerCommand(WorldPersistence worldPersistence, ServerConfig serverConfig, ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer) : base("swapSerializer", Perms.CONSOLE, "Swaps the world data serializer")
        {
            this.worldPersistence = worldPersistence;
            this.serverConfig = serverConfig;
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
        }

        protected override void Execute(CallArgs args)
        {
            serverConfig.SerializerMode = serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? ServerSerializerMode.JSON : ServerSerializerMode.PROTOBUF;
            worldPersistence.UpdateSerializer(serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? (IServerSerializer)protoBufSerializer : jsonSerializer);
            SendMessage(args.Sender, $"Swapped to {serverConfig.SerializerMode}");
        }
    }
}
