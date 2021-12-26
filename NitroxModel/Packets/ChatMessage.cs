using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class ChatMessage : Packet
    {
        [Index(0)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(1)]
        public virtual string Text { get; protected set; }
        public const ushort SERVER_ID = ushort.MaxValue;

        private ChatMessage() { }

        public ChatMessage(ushort playerId, string text)
        {
            PlayerId = playerId;
            Text = text;
        }
    }
}
