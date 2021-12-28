using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PDAEncyclopediaEntryAdd : Packet
    {
        [Index(0)]
        public virtual string Key { get; protected set; }

        public PDAEncyclopediaEntryAdd() { }

        public PDAEncyclopediaEntryAdd(string key)
        {
            Key = key;
        }
    }
}
