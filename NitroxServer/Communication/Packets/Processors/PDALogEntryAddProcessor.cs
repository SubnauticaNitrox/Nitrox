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

        public PDALogEntryAddProcessor(PlayerManager playerManager, PDAStateData pdaState)
        {
            this.playerManager = playerManager;
            this.pdaState = pdaState;
        }

        public override void Process(PDALogEntryAdd packet, Player player)
        {
            if (pdaState.PdaLog.Find(entry => entry.Key == packet.Key) == null)
            {
                pdaState.AddPDALogEntry(new PDALogEntry(packet.Key, packet.Timestamp));
            }
            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
