using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    internal sealed class PDALogEntryAddProcessor : AuthenticatedPacketProcessor<PDALogEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PdaStateData pdaState;
        private readonly ScheduleKeeper scheduleKeeper;

        public PDALogEntryAddProcessor(PlayerManager playerManager, PdaStateData pdaState, ScheduleKeeper scheduleKeeper)
        {
            this.playerManager = playerManager;
            this.pdaState = pdaState;
            this.scheduleKeeper = scheduleKeeper;
        }

        public override void Process(PDALogEntryAdd packet, Player player)
        {
            pdaState.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
            if (scheduleKeeper.ContainsScheduledGoal(packet.Key))
            {
                scheduleKeeper.UnScheduleGoal(packet.Key);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
