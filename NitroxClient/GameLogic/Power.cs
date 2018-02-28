﻿using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Power
    {
        private readonly IPacketSender packetSender;

        public Power(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ChargeChanged(string guid, float amount, PowerType powerType)
        {
            PowerLevelChanged powerChanged = new PowerLevelChanged(guid, amount, powerType);
            packetSender.Send(powerChanged);
            Log.Debug(powerChanged);
        }
    }
}
