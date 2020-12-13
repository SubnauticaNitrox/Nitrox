using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic
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
