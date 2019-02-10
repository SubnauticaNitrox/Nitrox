using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract, ProtoInclude(50, typeof(CorridorRotationMetadata)), ProtoInclude(60, typeof(MapRoomRotationMetadata))]
    public abstract class RotationMetadata
    {
        [ProtoIgnore]
        public Type GhostType { get; set; }

        public RotationMetadata(Type ghostType)
        {
            GhostType = ghostType;
        }
    }
}
