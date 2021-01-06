using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Serialization;
using NitroxModel.Server;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization;
using NitroxServer.Serialization.World;

namespace NitroxServer.ConsoleCommands
{
    internal sealed class SwapSerializerCommand : Command
    {
        private readonly WorldPersistence worldPersistence;
        private readonly ServerProtoBufSerializer protoBufSerializer;
        private readonly ServerJsonSerializer jsonSerializer;

        public SwapSerializerCommand(WorldPersistence worldPersistence, ServerProtoBufSerializer protoBufSerializer, ServerJsonSerializer jsonSerializer) : base("swapSerializer", Perms.CONSOLE, "Swaps the world data serializer")
        {
            this.worldPersistence = worldPersistence;
            this.protoBufSerializer = protoBufSerializer;
            this.jsonSerializer = jsonSerializer;
        }

        protected override void Execute(CallArgs args)
        {
            ServerConfig serverConfig = NitroxConfig.Deserialize<ServerConfig>();
            serverConfig.SerializerMode = serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? ServerSerializerMode.JSON : ServerSerializerMode.PROTOBUF;
            NitroxConfig.Serialize(serverConfig);

            worldPersistence.UpdateSerializer(serverConfig.SerializerMode == ServerSerializerMode.PROTOBUF ? (IServerSerializer)protoBufSerializer : jsonSerializer);
            SendMessage(args.Sender, $"Swapped to {serverConfig.SerializerMode}");
        }
    }
}
