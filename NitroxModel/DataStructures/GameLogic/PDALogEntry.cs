using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class PDALogEntry
    {
        [DataMember(Order = 1)]
        public string Key;

        [DataMember(Order = 2)]
        public float Timestamp;

        [IgnoreConstructor]
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
            return $"{nameof(Key)}: {Key}, {nameof(Timestamp)}: {Timestamp}";
        }
    }
}
