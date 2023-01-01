using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [DataContract]
    public class CyclopsFireData
    {
        [DataMember(Order = 1)]
        public NitroxId FireId { get; set; }

        [DataMember(Order = 2)]
        public NitroxId CyclopsId { get; set; }

        [DataMember(Order = 3)]
        public CyclopsRooms Room { get; set; }

        [DataMember(Order = 4)]
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
