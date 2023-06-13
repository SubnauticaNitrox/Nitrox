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

#if BELOWZERO
    public bool PostNotification { get; }

    public PDAEncyclopediaEntryAdd(string key, bool verbose, bool postNotification)
#elif SUBNAUTICA
    public PDAEncyclopediaEntryAdd(string key, bool verbose)
#endif
    {
        Key = key;
        Verbose = verbose;
#if BELOWZERO
        PostNotification = postNotification;
#endif
    }
}
