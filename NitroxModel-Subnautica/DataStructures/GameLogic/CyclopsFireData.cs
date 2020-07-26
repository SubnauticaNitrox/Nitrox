using System;
using NitroxModel.DataStructures;
using ProtoBufNet;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class CyclopsFireData
    {
        [ProtoMember(1)]
        public NitroxId FireId { get; set; }

        [ProtoMember(2)]
        public NitroxId CyclopsId { get; set; }

        [ProtoMember(3)]
        public CyclopsRooms Room { get; set; }

        [ProtoMember(4)]
        public int NodeIndex { get; set; }

        protected CyclopsFireData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public CyclopsFireData(NitroxId fireId, NitroxId cyclopsId, CyclopsRooms room, int nodeIndex)
        {
            FireId = fireId;
            CyclopsId = cyclopsId;
            Room = room;
            NodeIndex = nodeIndex;
        }

        public override string ToString()
        {
            return $"[CyclopsFireData - FireId: {FireId}, CyclopsId: {CyclopsId}, Room: {Room}, FireNodeIndex: {NodeIndex}]";
        }
    }
}
