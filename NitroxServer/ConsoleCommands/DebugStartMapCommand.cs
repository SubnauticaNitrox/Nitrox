using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Serialization.World;
using System.Collections.Generic;
using NitroxServer.GameLogic;
using NitroxModel.Packets;

namespace NitroxServer.ConsoleCommands
{
    internal class DebugStartMapCommand : Command
    {
        private readonly RandomStartGenerator nitroxRandomStart;
        private readonly PlayerManager playerManager;
        private readonly World world;

        public DebugStartMapCommand(PlayerManager playerManager, RandomStartGenerator nitroxRandomStart, World world) : base("debugstartmap", Perms.CONSOLE | Perms.DEBUG, "warning: spawns blocks")
        {
            this.playerManager = playerManager;
            this.nitroxRandomStart = nitroxRandomStart;
            this.world = world;
        }

        protected override void Execute(CallArgs args)
        {
            List<NitroxVector3> randomStartPositions = nitroxRandomStart.GenerateRandomStartPositions(world.Seed);

            playerManager.SendPacketToAllPlayers(new DebugStartMapPacket(randomStartPositions));
        }
    }
}
