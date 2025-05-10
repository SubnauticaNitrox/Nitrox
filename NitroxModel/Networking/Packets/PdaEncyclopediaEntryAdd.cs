using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PdaEncyclopediaEntryAdd : Packet
{
    public string Key { get; }
    /// <summary>
    ///     If true, shows a notification to the player.
    /// </summary>
    public bool Verbose { get; }

    public PdaEncyclopediaEntryAdd(string key, bool verbose)
    {
        Key = key;
        Verbose = verbose;
    }
}
