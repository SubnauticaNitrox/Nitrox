using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public abstract class RotationMetadata
    {
        [ProtoIgnore]
        public Type GhostType { get; set; }

        protected RotationMetadata(Type ghostType)
        {
            GhostType = ghostType;
        }

        public override string ToString()
        {
            return $"[RotationMetadata - GhostType: {GhostType}]";
        }
    }
}
