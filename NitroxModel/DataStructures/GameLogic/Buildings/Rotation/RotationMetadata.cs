using System;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [DynamicUnion]
    [ProtoContract]
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
