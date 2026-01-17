#if DEBUG
using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.Commands
{

    internal class DebugStartMapCommand : Command
    {
        private readonly RandomStartResource randomStartResource;
        private readonly IOptions<SubnauticaServerOptions> options;
        private readonly PlayerManager playerManager;

        public DebugStartMapCommand(PlayerManager playerManager, RandomStartResource randomStartResource, IOptions<SubnauticaServerOptions> options) :
            base("debugstartmap", Perms.ADMIN, "Spawns blocks at spawn positions")
        {
            this.playerManager = playerManager;
            this.randomStartResource = randomStartResource;
            this.options = options;
        }

        protected override void Execute(CallArgs args)
        {
            List<NitroxVector3> randomStartPositions = randomStartResource.RandomStartGenerator.GenerateRandomStartPositions(options.Value.Seed ?? throw new InvalidOperationException());

            playerManager.SendPacketToAllPlayers(new DebugStartMapPacket(randomStartPositions));
            SendMessage(args.Sender, $"Rendered {randomStartPositions.Count} spawn positions");
        }
    }

}
#endif
