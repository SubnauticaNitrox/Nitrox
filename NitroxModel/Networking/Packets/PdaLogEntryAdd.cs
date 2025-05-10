using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PdaLogEntryAdd : Packet
{
    public string Key { get; }
    public float Timestamp { get; }

    public PdaLogEntryAdd(string key, float timestamp)
    {
        Key = key;
        Timestamp = timestamp;
    }
}
