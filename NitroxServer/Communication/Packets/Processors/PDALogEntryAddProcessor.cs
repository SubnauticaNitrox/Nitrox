using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.Communication.Packets.Processors
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
            if (!pdaState.PdaLog.Any(entry => entry.Key == packet.Key))
            {
                pdaState.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
            }
            else
            {
                Log.Debug($"[PDALogEntryAddProcessor] There was an attempt of adding a duplicated entry in the PDALog [{packet.Key}]");
            }
            if (scheduleKeeper.ContainsScheduledGoal(packet.Key))
            {
                scheduleKeeper.UnScheduleGoal(packet.Key);
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
