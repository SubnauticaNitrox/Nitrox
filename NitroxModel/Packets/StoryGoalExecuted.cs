using System;

namespace NitroxModel.Packets;

[Serializable]
public class StoryGoalExecuted : Packet
{
    public string Key { get; }
    public EventType Type { get; }
    public float? Timestamp { get; set; }

    public StoryGoalExecuted(string key, EventType type, float? timestamp = null)
    {
        Key = key;
        Type = type;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return $"[StoryGoalExecuted Key: {Key}, Type: {Type}, Timestamp: {Timestamp}]";
    }

    public enum EventType
    {
        PDA,
        RADIO,
        ENCYCLOPEDIA,
        STORY
    }
}
