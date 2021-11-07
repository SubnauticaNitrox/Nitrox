using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [Serializable]
    [ProtoContract]
    public class MapRoomMetadata : BasePieceMetadata
    {
        [ProtoMember(1)]
        public NitroxId MapRoomFunctionalityId { get; set; }
        [ProtoMember(2)]
        public bool CameraDocked1 { get; set; }
        [ProtoMember(3)]
        public bool CameraDocked2 { get; set; }
        [ProtoMember(4)]
        public NitroxId Camera1NitroxId { get; set; }
        [ProtoMember(5)]
        public NitroxId Camera2NitroxId { get; set; }
        [ProtoMember(6)]
        public NitroxTechType TypeToScan { get; set; }

        protected MapRoomMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public MapRoomMetadata(NitroxId mapRoomFunctionalityId, bool cameraDocked1, bool cameraDocked2, NitroxId camera1NitroxId, NitroxId camera2NitroxId, NitroxTechType typeToScan, bool onlyUpdate = false, int payloadType = 0) : base()
        {
            MapRoomFunctionalityId = mapRoomFunctionalityId;
            CameraDocked1 = cameraDocked1;
            CameraDocked2 = cameraDocked2;
            Camera1NitroxId = camera1NitroxId;
            Camera2NitroxId = camera2NitroxId;
            TypeToScan = typeToScan;
            OnlyUpdate = onlyUpdate;
            PayloadType = payloadType;
            RefreshUpdatePayload();
        }
        public MapRoomMetadata(NitroxId mapRoomFunctionalityId, bool cameraDocked1, bool cameraDocked2, NitroxId camera1NitroxId, NitroxId camera2NitroxId) : this(mapRoomFunctionalityId, cameraDocked1, cameraDocked2, camera1NitroxId, camera2NitroxId, null, true, 1)
        { }

        public MapRoomMetadata(NitroxId mapRoomFunctionalityId, NitroxTechType typeToScan) : this(mapRoomFunctionalityId, false, false, null, null, typeToScan, true, 2)
        { }

        public override void RefreshUpdatePayload()
        {
            switch (PayloadType)
            {
                case 1:
                    UpdatePayload = new object[] { CameraDocked1, CameraDocked2, Camera1NitroxId, Camera2NitroxId };
                    break;
                case 2:
                    UpdatePayload = new object[] { TypeToScan };
                    break;
            }
            
        }

        public override BasePieceMetadata LoadUpdatePayload(object[] updatePayload, int payloadType)
        {
            switch (payloadType)
            {
                case 1:
                    CameraDocked1 = (bool)updatePayload[0];
                    CameraDocked2 = (bool)updatePayload[1];
                    Camera1NitroxId = (NitroxId)updatePayload[2];
                    Camera2NitroxId = (NitroxId)updatePayload[3];
                    break;
                case 2:
                    TypeToScan = (NitroxTechType)updatePayload[0];
                    break;
            }
            return this;
        }

        public override string ToString()
        {
            return $"[CameraDockingMetadata - MapRoomFunctionalityId: {MapRoomFunctionalityId}, CameraDocked1: {CameraDocked1}, CameraDocked2: {CameraDocked2}, Camera1NitroxId: {Camera1NitroxId}, Camera2NitroxId: {Camera2NitroxId}, TypeToScan: {TypeToScan}]";
        }
    }
}
