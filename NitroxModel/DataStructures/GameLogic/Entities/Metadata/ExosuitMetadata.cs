using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class ExosuitMetadata : VehicleMetadata
{
    [IgnoreConstructor]
    protected ExosuitMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ExosuitMetadata(float health, string name, NitroxVector3[] colors) : base(health, name, colors)
    { }

    public override string ToString()
    {
        return $"[{nameof(ExosuitMetadata)} {base.ToString()}]";
    }
}
