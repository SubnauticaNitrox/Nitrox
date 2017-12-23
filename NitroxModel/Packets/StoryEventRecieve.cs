using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class StoryEventRecieve : Packet
    {
        public StoryEventType StoryEventType { get; }
        public string Key { get; }

        public StoryEventRecieve(StoryEventType storyEventType, string key = "") : base()
        {
            this.StoryEventType = storyEventType;
            this.Key = key;
        }
    }
}
