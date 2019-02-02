using System;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class CyclopsFireData
    {
        [ProtoMember(1)]
        public string FireGuid { get; set; }

        [ProtoMember(2)]
        public string CyclopsGuid { get; set; }

        [ProtoMember(3)]
        public CyclopsRooms Room { get; set; }

        [ProtoMember(4)]
        public int NodeIndex { get; set; }

        public CyclopsFireData()
        {

        }

        public CyclopsFireData(string fireGuid, string cyclopsGuid, CyclopsRooms room, int nodeIndex)
        {
            FireGuid = fireGuid;
            CyclopsGuid = cyclopsGuid;
            Room = room;
            NodeIndex = nodeIndex;
        }

        public override string ToString()
        {
            return "[CyclopsFireData"
                + " FireGuid: " + FireGuid
                + " CyclopsGuid: " + CyclopsGuid
                + " Room: " + Room
                + " FireNodeIndex: " + NodeIndex
                + "]";
        }
    }
}
