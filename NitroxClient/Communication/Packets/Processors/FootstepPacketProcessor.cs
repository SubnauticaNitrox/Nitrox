using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Gui.HUD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;
public class FootstepPacketProcessor : ClientPacketProcessor<FootstepPacket>
{
    private readonly float footstepAudioRange;
    public FootstepPacketProcessor(float footstepAudioRange)
    {
        this.footstepAudioRange = footstepAudioRange;
    }
    public override void Process(FootstepPacket packet)
    {
        Log.Info("Processing footstep packet on client");
    }
}
