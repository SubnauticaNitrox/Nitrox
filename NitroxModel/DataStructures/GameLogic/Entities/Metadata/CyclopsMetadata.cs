using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class CyclopsMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool FloodLightsOn { get; set; }

    [DataMember(Order = 2)]
    public bool InternalLightsOn { get; set; }

    [DataMember(Order = 3)]
    public bool SilentRunningOn { get; set; }

    [DataMember(Order = 4)]
    public bool ShieldOn { get; set; }

    [DataMember(Order = 5)]
    public bool SonarOn { get; set; }

    [DataMember(Order = 6)]
    public bool EngineOn { get; set; }

    [DataMember(Order = 7)]
    public int EngineMode { get; set; }

    [DataMember(Order = 8)]
    public float Health { get; set; }

    [IgnoreConstructor]
    protected CyclopsMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsMetadata(bool floodLightsOn, bool internalLightsOn, bool silentRunningOn, bool shieldOn, bool sonarOn, bool engineOn, int engineMode, float health)
    {
        FloodLightsOn = floodLightsOn;
        InternalLightsOn = internalLightsOn;
        SilentRunningOn = silentRunningOn;
        ShieldOn = shieldOn;
        SonarOn = sonarOn;
        EngineOn = engineOn;
        EngineMode = engineMode;
        Health = health;
    }

    public override string ToString()
    {
        return $"[CyclopsMetadata FloodLightsOn: {FloodLightsOn} InternalLightsOn: {InternalLightsOn} SilentRunningOn: {SilentRunningOn} ShieldOn: {ShieldOn} SonarOn: {SonarOn} EngineOn: {EngineOn} EngineMode: {EngineMode} Health: {Health}]";
    }
}
