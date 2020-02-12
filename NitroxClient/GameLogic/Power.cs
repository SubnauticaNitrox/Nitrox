using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
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

        public void ChargeChanged(NitroxId id, float amount, PowerType powerType)
        {
            PowerLevelChanged powerChanged = new PowerLevelChanged(id, amount, powerType);
            packetSender.Send(powerChanged);
            Log.Instance.LogMessage(LogCategory.Debug, $"{powerChanged}");
        }
    }
}
