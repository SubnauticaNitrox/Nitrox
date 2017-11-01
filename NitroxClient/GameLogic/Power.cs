using NitroxClient.Communication;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Power
    {
        private readonly PacketSender packetSender;

        public Power(PacketSender packetSender)
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
