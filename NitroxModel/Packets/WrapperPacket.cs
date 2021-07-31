namespace NitroxModel.Packets
{
    /**
     * WrapperPacket for LiteNetLib implementation
     * Because of the LiteNetLib serializer we can't deserialize the incoming bytes and have
     * to use their serializer. This packet is the bridge between the Nitrox packet serializer
     * and the LiteNetLib serializer. In the LiteNetLib implementation, every packet is wrapped
     * and then uses Nitrox serializer with the packetData.
     */
    public class WrapperPacket
    {
        public byte[] packetData { get; set; }

        public WrapperPacket()
        {
        }

        public WrapperPacket(byte[] packetData)
        {
            this.packetData = packetData;
        }
    }
}
