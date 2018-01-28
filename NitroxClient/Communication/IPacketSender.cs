using NitroxModel.Packets;

namespace NitroxClient.Communication
{
    public interface IPacketSender
    {
        void send(Packet packet);
        PacketSuppression<T> suppress<T>();

        //I loath that I must polute this interface with this icky-poo member, but it is necessary given that this feature already has a number of broad-stroke changes
        //and the current PacketSender oddly carries this responsibility.
        //For the love of all that is good and just in the world, we need a champion to step forth and slay this horrific machination by relocating it to an appropriate
        //global user context object.
        string PlayerId { get; }
    }
}
