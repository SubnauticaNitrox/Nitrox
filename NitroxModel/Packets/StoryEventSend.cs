using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class StoryEventSend : Packet
    {
        [Index(0)]
        public virtual EventType Type { get; protected set; }
        [Index(1)]
        public virtual string Key { get; protected set; }

        private StoryEventSend() { }

        public StoryEventSend(EventType type, string key = "")
        {
            Type = type;
            Key = key;
        }

        public enum EventType
        {
            PDA,
            RADIO,
            ENCYCLOPEDIA,
            STORY,
            GOAL_UNLOCK,
            EXTRA,
            PDA_EXTRA,
        }
    }
}
