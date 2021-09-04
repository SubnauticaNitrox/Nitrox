using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PDAEncyclopediaEntryAdd : Packet
    {
        public string Key { get; }
#if BELOWZERO
        public bool PostNotification { get; }

        public PDAEncyclopediaEntryAdd(string key, bool postNotification)
#elif SUBNAUTICA
        public PDAEncyclopediaEntryAdd(string key)
#endif
        {
            Key = key;
#if BELOWZERO
            PostNotification = postNotification;
#endif
        }
    }
}
