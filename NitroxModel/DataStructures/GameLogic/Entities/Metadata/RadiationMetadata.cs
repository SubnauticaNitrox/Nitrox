using System;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable, DataContract]
public class RadiationMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public float Health { get; set; }

    [DataMember(Order = 2)]
    public float FixRealTime { get; set; }

    public RadiationMetadata(float health, float fixRealTime = -1)
    {
        Health = health;
        FixRealTime = fixRealTime;
    }
}
