using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StoryEventSend : Packet
    {
        public StoryEventType StoryEventType { get; }
        public string Key { get; }

        public StoryEventSend(StoryEventType storyEventType, string key = "") : base()
        {
            StoryEventType = storyEventType;
            Key = key;
        }

        public override string ToString()
        {
            return "[StoryEventSend - StoryEventType: " + StoryEventType + " Key: " + Key + "]";
        }
    }

    public enum StoryEventType
    {
        PDA,
        RADIO,
        ENCYCLOPEDIA,
        STORY,
        EXTRA,
        GOAL_UNLOCK
    }
}
