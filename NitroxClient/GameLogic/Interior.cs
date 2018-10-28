using NitroxClient.Communication.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class Interior
    {
        private readonly IPacketSender packetSender;
        private readonly INitroxLogger log;

        public Interior(IPacketSender packetSender, INitroxLogger logger)
        {
            this.packetSender = packetSender;
            log = logger;
        }

        public void OpenableStateChanged(string guid, bool isOpen, float animationDuration)
        {
            OpenableStateChanged stateChange = new OpenableStateChanged(guid, isOpen, animationDuration);
            packetSender.Send(stateChange);
            log.Debug(stateChange.ToString());
        }
    }
}
