using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Power
    {
        private readonly IPacketSender packetSender;
        private readonly INitroxLogger log;

        public Power(IPacketSender packetSender, INitroxLogger logger)
        {
            this.packetSender = packetSender;
            log = logger;
        }

        public void ChargeChanged(string guid, float amount, PowerType powerType)
        {
            PowerLevelChanged powerChanged = new PowerLevelChanged(guid, amount, powerType);
            packetSender.Send(powerChanged);
            log.Debug(powerChanged.ToString());
        }
    }
}
