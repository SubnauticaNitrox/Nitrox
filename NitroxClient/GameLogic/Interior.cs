﻿using NitroxClient.Communication.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Interior
    {
        private readonly IPacketSender packetSender;

        public Interior(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void OpenableStateChanged(string guid, bool isOpen, float animationDuration)
        {
            OpenableStateChanged stateChange = new OpenableStateChanged(guid, isOpen, animationDuration);
            packetSender.Send(stateChange);
            Log.Debug(stateChange);
        }
    }
}
