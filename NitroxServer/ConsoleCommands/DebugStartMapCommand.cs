using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxModel.DataStructures;
using NitroxServer.Serialization.World;
using System.Collections.Generic;
using NitroxServer.GameLogic;
using NitroxModel.Packets;

namespace NitroxServer.ConsoleCommands
{
    internal class DebugStartMapCommand : Command
    {
        private readonly PlayerManager playerManager;
        private readonly NitroxRandomStart nitroxRandomStart;
        private readonly World world;

        public DebugStartMapCommand(PlayerManager playerManager, NitroxRandomStart nitroxRandomStart, World world) : base("debugstartmap", Perms.CONSOLE, "warning: spawns blocks")
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
