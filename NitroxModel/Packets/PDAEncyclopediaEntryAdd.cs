using System;

namespace NitroxModel.Packets;

[Serializable]
public class PDAEncyclopediaEntryAdd : Packet
{
    public string Key { get; }
    /// <summary>
    ///     If true, shows a notification to the player.
    /// </summary>
    public bool Verbose { get; }

    public PDAEncyclopediaEntryAdd(string key, bool verbose)
    {
        Key = key;
        Verbose = verbose;
    }
}
