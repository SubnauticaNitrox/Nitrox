using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class CyclopsMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public bool SilentRunningOn { get; set; }

    [DataMember(Order = 2)]
    public bool ShieldOn { get; set; }

    [DataMember(Order = 3)]
    public bool SonarOn { get; set; }

    [DataMember(Order = 4)]
    public bool EngineOn { get; set; }

    [DataMember(Order = 5)]
    public int EngineMode { get; set; }

    [DataMember(Order = 6)]
    public float Health { get; set; }

    [DataMember(Order = 7)]
    public bool IsDestroyed { get; set; }

    [IgnoreConstructor]
    protected CyclopsMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public CyclopsMetadata(bool silentRunningOn, bool shieldOn, bool sonarOn, bool engineOn, int engineMode, float health, bool isDestroyed)
    {
        SilentRunningOn = silentRunningOn;
        ShieldOn = shieldOn;
        SonarOn = sonarOn;
        EngineOn = engineOn;
        EngineMode = engineMode;
        Health = health;
        IsDestroyed = isDestroyed;
    }

    public override string ToString()
    {
        return $"[CyclopsMetadata SilentRunningOn: {SilentRunningOn}, ShieldOn: {ShieldOn}, SonarOn: {SonarOn}, EngineOn: {EngineOn}, EngineMode: {EngineMode}, Health: {Health}, IsDestroyed: {IsDestroyed}]";
    }
}
