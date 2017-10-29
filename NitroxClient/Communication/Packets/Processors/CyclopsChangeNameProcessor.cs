using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeNameProcessor : ClientPacketProcessor<CyclopsChangeName>
    {
        private readonly PacketSender packetSender;

        public CyclopsChangeNameProcessor(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeName namePacket)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(namePacket.Guid);
            CyclopsNameScreenProxy ScreenProxy = cyclops.RequireComponentInChildren<CyclopsNameScreenProxy>();
            SubName subname = (SubName)ScreenProxy.subNameInput.ReflectionGet("target");

            if (subname != null)
            {
                using (packetSender.Suppress<CyclopsChangeName>())
                {
                    subname.SetName(namePacket.Name);
                    ScreenProxy.subNameInput.inputField.text = namePacket.Name;
                }
            }
            else
            {
                Log.Error("Could not find SubName via target reflection");
            }
        }
    }
}
