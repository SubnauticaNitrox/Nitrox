using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(SignMetadata))]
    [ProtoInclude(100, typeof(MapRoomMetadata))]
    public abstract class BasePieceMetadata
    {
        public bool OnlyUpdate;
        public object[] UpdatePayload;
        public int PayloadType;
        public abstract void RefreshUpdatePayload();
        public abstract BasePieceMetadata LoadUpdatePayload(object[] updatePayload, int payloadType);
    }
}
