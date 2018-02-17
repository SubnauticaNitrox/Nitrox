using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IPacketSender
    {
        void Send(Packet packet);
        PacketSuppressor<T> Suppress<T>();
    }
}
