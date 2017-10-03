using NitroxClient.Communication;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System;

namespace NitroxClient.GameLogic
{
    public class Power
    {
        private PacketSender packetSender;

        public Power(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void ChargeChanged(String guid, float amount, PowerType powerType)
        {
            PowerLevelChanged powerChanged = new PowerLevelChanged(packetSender.PlayerId, guid, amount, powerType);
            packetSender.Send(powerChanged);
            Log.Debug(powerChanged);
        }
    }
}
