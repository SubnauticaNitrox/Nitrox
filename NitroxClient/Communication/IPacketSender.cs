using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public interface IPacketSender
    {
        void send(Packet packet);
    }
}
