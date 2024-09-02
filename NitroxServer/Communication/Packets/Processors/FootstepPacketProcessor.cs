using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using System.Numerics;

namespace NitroxServer.Communication.Packets.Processors
{
    public class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
    {
        private readonly float footstepAudioRange;
        private readonly PlayerManager playerManager;
        public FootstepPacketProcessor(float footstepAudioRange, PlayerManager playerManager)
        {
            this.footstepAudioRange = footstepAudioRange;
            this.playerManager = playerManager;
        }
        public override void Process(FootstepPacket footstepPacket, Player sendingPlayer)
        {
            Log.Info("Processing footstep packet on server from " + sendingPlayer.Name);
            var players = playerManager.GetAllPlayers();
            foreach(Player player in players)
            {
                if(NitroxVector3.Distance(player.Position, sendingPlayer.Position) <= footstepAudioRange)
                {
                    // Forward footstep packet to players if they are within range to hear it
                    playerManager.SendPacketToOtherPlayers(footstepPacket, sendingPlayer);
                }
            }
        }
    }
}
