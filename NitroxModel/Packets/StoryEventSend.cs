using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StoryEventSend : Packet
    {
        public StoryEventType StoryEventType { get; }
        public string Key { get; }

        public StoryEventSend(StoryEventType storyEventType, string key = "")
        {
            StoryEventType = storyEventType;
            Key = key;
        }

        public override string ToString()
        {
            return $"[StoryEventSend - StoryEventType: {StoryEventType}, Key: {Key}]";
        }
    }
}
