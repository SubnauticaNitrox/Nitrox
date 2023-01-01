using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata
{
    [Serializable]
    [DataContract]
    public class BaseModuleBuilderMetadata : BuilderMetadata
    {
        // Base modules anchor based on a face.  This can be constructed via these two attributes.
        [DataMember(Order = 1)]
        public NitroxInt3 Cell { get; set; }

        [DataMember(Order = 2)]
        public int Direction { get; set; }

        [IgnoreConstructor]
        protected BaseModuleBuilderMetadata()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public BaseModuleBuilderMetadata(NitroxInt3 cell, int direction)
        {
            Cell = cell;
            Direction = direction;
        }

        public override string ToString()
        {
            return $"[BaseModuleRotationMetadata - Cell: {Cell}, Direction: {Direction}]";
        }
    }
}
