using NitroxModel.Packets;

namespace NitroxClient.Debuggers.Interfaces
{
    public interface IPacketSendListener
    {
        void OnPacketSent(Packet packet);
    }
}
