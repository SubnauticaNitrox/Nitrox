using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class PDALogEntry
    {
        [ProtoMember(1)]
        public string Key;

        [ProtoMember(2)]
        public float Timestamp;

        public PDALogEntry()
        {
            // Default Constructor for serialization
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
