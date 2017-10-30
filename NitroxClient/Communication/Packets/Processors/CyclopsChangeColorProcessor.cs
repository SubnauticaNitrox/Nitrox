using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using NitroxModel.Helper;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeColorProcessor : ClientPacketProcessor<CyclopsChangeColor>
    {
        private readonly PacketSender packetSender;

        public CyclopsChangeColorProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeColor colorPacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(colorPacket.Guid);
            CyclopsNameScreenProxy screenProxy = cyclops.RequireComponentInChildren<CyclopsNameScreenProxy>();
            SubName subname = (SubName)screenProxy.subNameInput.ReflectionGet("target");

            if (subname != null)
            {
                subname.SetColor(colorPacket.Index, colorPacket.HSB, colorPacket.Color);
                screenProxy.subNameInput.ReflectionCall("SetColor", false, false, colorPacket.Index, colorPacket.Color);
                screenProxy.subNameInput.SetSelected(colorPacket.Index);
            }
            else
            {
                Log.Error("Could not find SubName in SubNameInput to change color");
            }
        }

    }
}
