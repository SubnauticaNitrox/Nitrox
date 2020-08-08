using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class PDALogEntry
    {
        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public float Timestamp { get; set; }

        protected PDALogEntry()
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
            return $"[PDALogEntry - Key: {Key} Timestamp: {Timestamp}]";
        }
    }
}
