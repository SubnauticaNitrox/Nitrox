using LiteNetLib.Utils;

namespace NitroxModel.Packets
{
    /**
     * WrapperPacket for LiteNetLib implementation
     * Because of the LiteNetLib serializer we can't deserialize the incoming bytes and have
     * to use their serializer. This packet is the bridge between the Nitrox packet serializer
     * and the LiteNetLib serializer. In the LiteNetLib implementation, every packet is wrapped
     * and then uses Nitrox serializer with the packetData.
     */
    public class WrapperPacket : INetSerializable
    {
        public byte[] PacketData { get; set; }

        public WrapperPacket()
        {
        }

        public WrapperPacket(byte[] packetData)
        {
            PacketData = packetData;
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PacketData.Length);
            writer.Put(PacketData);
        }

        public void Deserialize(NetDataReader reader)
        {
            int packetDataLength = reader.GetInt();
            PacketData = new byte[packetDataLength];
            reader.GetBytes(PacketData, packetDataLength);
        }
    }
}
