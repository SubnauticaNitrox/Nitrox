using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public abstract class BuilderMetadata
    {
        [ProtoIgnore]
        public Type GhostType { get; set; }

        public BuilderMetadata(Type ghostType)
        {
            GhostType = ghostType;
        }
    }
}
