using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
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

        public void OpenableStateChanged(NitroxId id, bool isOpen, float animationDuration)
        {
            OpenableStateChanged stateChange = new OpenableStateChanged(id, isOpen, animationDuration);
            packetSender.Send(stateChange);
            Log.Debug(stateChange);
        }
    }
}
