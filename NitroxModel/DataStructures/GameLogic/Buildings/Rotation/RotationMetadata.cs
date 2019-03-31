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

        public RotationMetadata(Type ghostType)
        {
            GhostType = ghostType;
        }
    }
}
