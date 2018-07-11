using System;
using UnityEngine;
using System.Collections.Generic;
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
    }
}
