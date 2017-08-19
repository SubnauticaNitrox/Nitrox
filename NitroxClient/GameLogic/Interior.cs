using NitroxClient.Communication;
using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic
{
    public class Interior
    {
        private PacketSender packetSender;

        public Interior(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OpenableStateChanged(String guid, bool isOpen, float animationDuration)
        {
            OpenableStateChanged stateChange = new OpenableStateChanged(packetSender.PlayerId, guid, isOpen, animationDuration);
            packetSender.Send(stateChange);
            Console.WriteLine(stateChange);
        }
    }
}
