using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata;

[Serializable]
[DataContract]
public class RocketMetadata : EntityMetadata
{
    [DataMember(Order = 1)]
    public int CurrentStage { get; set; }

    [DataMember(Order = 2)]
    public float LastStageTransitionTime { get; set; }

    [DataMember(Order = 3)]
    public int ElevatorState { get; set; }

    [DataMember(Order = 4)]
    public float ElevatorPosition { get; set; }

    [DataMember(Order = 5)]
    public List<int> PreflightChecks { get; set; } = new();

    [IgnoreConstructor]
    protected RocketMetadata()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    public RocketMetadata(int currentStage, float lastStageTransitionTime, int elevatorState, float elevatorPosition, List<int> preflightChecks)
    {
        CurrentStage = currentStage;
        LastStageTransitionTime = lastStageTransitionTime;
        ElevatorState = elevatorState;
        ElevatorPosition = elevatorPosition;
        PreflightChecks = preflightChecks;
    }

    public override string ToString()
    {
        return $"[{nameof(RocketMetadata)} CurrentStage: {CurrentStage}, LastStageTransitionTime: {LastStageTransitionTime}, ElevatorState: {ElevatorState}, ElevatorPosition: {ElevatorPosition}, PreflightChecks: {string.Join(",", PreflightChecks)}]";
    }
}
