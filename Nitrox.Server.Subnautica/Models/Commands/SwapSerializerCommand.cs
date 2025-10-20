using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Server;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class SwapSerializerCommand : Command
    {
        private readonly WorldService worldService;
        private readonly IOptions<SubnauticaServerOptions> options;

        public SwapSerializerCommand(IOptions<SubnauticaServerOptions> options, WorldService worldService) : base("swapserializer", Perms.HOST, "Allows to change the save format")
        {
            AddParameter(new TypeEnum<ServerSerializerMode>("serializer", true, "Save format to change to"));

            this.worldService = worldService;
            this.options = options;
        }

        protected override void Execute(CallArgs args)
        {
            ServerSerializerMode serializerMode = args.Get<ServerSerializerMode>(0);

            if (serializerMode != options.Value.SerializerMode)
            {
                options.Value.SerializerMode = serializerMode;
                worldService.UpdateSerializer(serializerMode);
                SendMessage(args.Sender, $"Server save format swapped to {options.Value.SerializerMode}");
            }
            else
            {
                SendMessage(args.Sender, "Server is already using this save format");
            }
        }
    }
}
