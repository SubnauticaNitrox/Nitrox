using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
