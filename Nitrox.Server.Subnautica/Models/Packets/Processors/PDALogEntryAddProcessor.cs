using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors
{
    public class PDALogEntryAddProcessor : AuthenticatedPacketProcessor<PDALogEntryAdd>
    {
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaState;
        private readonly ScheduleKeeper scheduleKeeper;

        public PDALogEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaState, ScheduleKeeper scheduleKeeper)
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
