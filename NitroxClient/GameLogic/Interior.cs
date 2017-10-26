using NitroxClient.Communication;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic
{
    public class Interior
    {
        private readonly PacketSender packetSender;

        public Interior(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OpenableStateChanged(String guid, bool isOpen, float animationDuration)
        {
            OpenableStateChanged stateChange = new OpenableStateChanged(packetSender.PlayerId, guid, isOpen, animationDuration);
            packetSender.Send(stateChange);
            Log.Debug(stateChange);
        }
    }
}
