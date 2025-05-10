using NitroxModel.Networking.Packets;

namespace Nitrox.Test.Client.Communication
{
    [Serializable]
    public record TestNonActionPacket : Packet
    {
        public ushort PlayerId { get; }

        public TestNonActionPacket(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
