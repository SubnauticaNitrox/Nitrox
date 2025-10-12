#if DEBUG
using System.Collections.Generic;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Serialization.World;

namespace Nitrox.Server.Subnautica.Models.Commands
{

    internal class DebugStartMapCommand : Command
    {
        private readonly RandomStartGenerator nitroxRandomStart;
        private readonly PlayerManager playerManager;
        private readonly World world;

        public DebugStartMapCommand(PlayerManager playerManager, RandomStartGenerator nitroxRandomStart, World world) :
            base("debugstartmap", Perms.ADMIN, "Spawns blocks at spawn positions")
        {
            this.playerManager = playerManager;
            this.nitroxRandomStart = nitroxRandomStart;
            this.world = world;
        }

        protected override void Execute(CallArgs args)
        {
            List<NitroxVector3> randomStartPositions = nitroxRandomStart.GenerateRandomStartPositions(world.Seed);

            playerManager.SendPacketToAllPlayers(new DebugStartMapPacket(randomStartPositions));
            SendMessage(args.Sender, $"Rendered {randomStartPositions.Count} spawn positions");
        }
    }

}
#endif
