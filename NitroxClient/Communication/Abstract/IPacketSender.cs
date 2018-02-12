using NitroxModel.Packets;

namespace NitroxClient.Communication.Abstract
{
    public interface IPacketSender
    {
        void Send(Packet packet);
        PacketSuppression<T> Suppress<T>();
    }
}
