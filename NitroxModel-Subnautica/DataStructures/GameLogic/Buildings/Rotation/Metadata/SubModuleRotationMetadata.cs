using ProtoBufNet;
using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation
{
    [Serializable]
    [ProtoContract]
    public class SubModuleRotationMetadata : RotationMetadata
    {
        public const long VERSION = 1;

        [ProtoMember(1)]
        public NitroxVector3 ParentPosition { get; set; }

        [ProtoMember(2)]
        public NitroxQuaternion ParentRotation { get; set; }

        public SubModuleRotationMetadata()
        {
            // For serialization purposes
        }

        public SubModuleRotationMetadata(NitroxVector3 ParentPosition, NitroxQuaternion ParentRotation)
        {
            this.ParentPosition = ParentPosition;
            this.ParentRotation = ParentRotation;
        }

        public override string ToString()
        {
            return "[SubModuleRotationMetadata ParentPosition: " + ParentPosition + " ParentRotation: " + ParentRotation + "]";
        }
    }
}
