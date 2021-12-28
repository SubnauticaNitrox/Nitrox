using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PDALogEntryAdd : Packet
    {
        [Index(0)]
        public virtual string Key { get; protected set; }
        [Index(1)]
        public virtual float Timestamp { get; protected set; }

        public PDALogEntryAdd() { }

        public PDALogEntryAdd(string key, float timestamp)
        {
            Key = key;
            Timestamp = timestamp;
        }
    }
}
