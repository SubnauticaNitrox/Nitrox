using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StoryEventSend : Packet
    {
        public EventType Type { get; }
        public string Key { get; }

        public StoryEventSend(EventType type, string key = "")
        {
            Type = type;
            Key = key;
        }

        public enum EventType
        {
            PDA,
            PDA_EXTRA,
            RADIO,
            ENCYCLOPEDIA,
            STORY,
            EXTRA,
            GOAL_UNLOCK
        }
    }
}
