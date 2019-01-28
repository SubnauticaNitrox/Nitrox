using System;
using NitroxModel.DataStructures.Util;
using ProtoBuf;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class FireData
    {
        [ProtoMember(1)]
        public string FireGuid { get; set; }

        [ProtoMember(2)]
        public string SerializableCyclopsGuid
        {
            get { return (CyclopsGuid.IsPresent()) ? CyclopsGuid.Get() : null; }
            set { CyclopsGuid = Optional<string>.OfNullable(value); }
        }
        [ProtoIgnore]
        public Optional<string> CyclopsGuid { get; set; }


        [ProtoMember(3)]
        public CyclopsRooms Room { get; set; }
        /*public CyclopsRooms? SerializableRoom
        {
            get { return (Room.IsPresent()) ? (CyclopsRooms?)Room.Get() : null; }
            set { Room = value == null ? Optional<CyclopsRooms>.Empty() : Optional<CyclopsRooms>.OfNullable((CyclopsRooms)value); }
        }
        [ProtoIgnore]
        public Optional<CyclopsRooms> Room { get; set; }*/

        [ProtoMember(4)]
        public int NodeIndex { get; set; }
        /*public int? SerializableNodeIndex
        {
            get { return (NodeIndex.IsPresent()) ? (int?)NodeIndex.Get() : null; }
            set { NodeIndex = value == null ? Optional<int>.Empty() : Optional<int>.OfNullable((int)value); }
        }
        [ProtoIgnore]
        public Optional<int> NodeIndex { get; set; }*/

        public FireData()
        {
            CyclopsGuid = Optional<string>.Empty();
        }

        public FireData(string fireGuid, Optional<string> cyclopsGuid, CyclopsRooms room, int nodeIndex)
        {
            FireGuid = fireGuid;
            CyclopsGuid = cyclopsGuid;
            Room = room;
            NodeIndex = nodeIndex;
        }

        public override string ToString()
        {
            return "[FireData"
                + " FireGuid: " + FireGuid
                + " CyclopsGuid: " + (CyclopsGuid.IsPresent() ? CyclopsGuid.Get() : "None")
                + " Room: " + Room
                + " FireNodeIndex: " + NodeIndex
                //+ " Room: " + (Room.IsPresent() ? Room.Get().ToString() : "None")
                //+ " FireNodeIndex: " + (NodeIndex.IsPresent() ? NodeIndex.Get().ToString() : "None")
                + "]";
        }
    }
}
