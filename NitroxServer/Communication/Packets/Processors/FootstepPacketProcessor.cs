using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxServer.Communication.Packets.Processors
{
    public class FootstepPacketProcessor : AuthenticatedPacketProcessor<FootstepPacket>
    {
        private readonly float footstepAudioRange;
        public FootstepPacketProcessor(float footstepAudioRange)
        {
            this.footstepAudioRange = footstepAudioRange;
        }
        public override void Process(FootstepPacket packet, Player sendingPlayer)
        {
            Log.Info("Processing footstep packet on server from " + sendingPlayer.Name);
        }
    }
}
