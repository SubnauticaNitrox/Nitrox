using NitroxModel.DataStructures;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class CyclopsFireData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxId FireId { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxId CyclopsId { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual CyclopsRooms Room { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual int NodeIndex { get; set; }

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
