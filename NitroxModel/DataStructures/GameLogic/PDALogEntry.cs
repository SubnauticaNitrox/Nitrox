using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class PDALogEntry
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual string Key { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual float Timestamp { get; set; }

        public PDALogEntry()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public PDALogEntry(string key, float timestamp)
        {
            Key = key;
            Timestamp = timestamp;
        }

        public override string ToString()
        {
            return $"{nameof(Key)}: {Key}, {nameof(Timestamp)}: {Timestamp}";
        }
    }
}
