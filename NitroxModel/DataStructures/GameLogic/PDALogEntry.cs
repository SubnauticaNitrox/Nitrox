using System;
using ProtoBuf;

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
    }
}
