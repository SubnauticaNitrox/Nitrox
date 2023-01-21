using System;

namespace NitroxModel.Packets;

[Serializable]
public class PDAEncyclopediaEntryAdd : Packet
{
    public string Key { get; }
    public bool Verbose { get; }

    public PDAEncyclopediaEntryAdd(string key, bool verbose)
    {
        Key = key;
        Verbose = verbose;
    }
}
