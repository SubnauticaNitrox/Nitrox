namespace NitroxModel.Packets
{
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
