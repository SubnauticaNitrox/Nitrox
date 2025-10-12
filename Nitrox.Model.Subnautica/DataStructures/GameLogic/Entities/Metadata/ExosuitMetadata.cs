using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class ExosuitMetadata : VehicleMetadata
{
    [IgnoreConstructor]
    protected ExosuitMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public ExosuitMetadata(float health, bool inPrecursor, string name, NitroxVector3[] colors) : base(health, inPrecursor, name, colors)
    { }

    public override string ToString()
    {
        return $"[{nameof(ExosuitMetadata)} {base.ToString()}]";
    }
}
