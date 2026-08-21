using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class LiveMixinMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Health { get; set; }

    [IgnoreConstructor]
    protected LiveMixinMetadata()
    {
        // Constructor for serialisation
    }

    public LiveMixinMetadata(float health)
    {
        Health = health;
    }

    public override string ToString()
    {
        return $"[{nameof(LiveMixinMetadata)} Health: {Health}]";
    }
}
