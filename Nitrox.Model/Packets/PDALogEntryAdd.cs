using System;

namespace Nitrox.Model.Packets;

[Serializable]
public class PDALogEntryAdd : Packet
{
    public string Key { get; }
    public float Timestamp { get; }

    public PDALogEntryAdd(string key, float timestamp)
    {
        Key = key;
        Timestamp = timestamp;
    }
}
