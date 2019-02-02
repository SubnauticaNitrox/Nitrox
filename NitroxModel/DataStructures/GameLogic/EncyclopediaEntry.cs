using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class EncyclopediaEntry
    {
        [ProtoMember(1)]
        public string Key { get; set; }

        [ProtoMember(2)]
        public bool IsTimeCapsule { get; set; }

        public EncyclopediaEntry(string key, bool isTimeCapsule)
        {
            Key = key;
            IsTimeCapsule = isTimeCapsule;
        }

        public override string ToString()
        {
            return $"Key: {Key}, Is Time Capsule: {IsTimeCapsule}";
        }
    }
}
