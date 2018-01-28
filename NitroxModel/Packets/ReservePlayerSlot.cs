namespace NitroxModel.Packets
{
    public class ReservePlayerSlot : Packet
    {
        public string CorrelationId { get; private set; }
        public string PlayerName { get; private set; }

        public ReservePlayerSlot(string correlationId, string playerName)
        {
            CorrelationId = correlationId;
            PlayerName = playerName;
        }
    }
}