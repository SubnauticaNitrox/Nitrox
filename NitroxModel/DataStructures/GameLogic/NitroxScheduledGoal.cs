using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class NitroxScheduledGoal
{
    [DataMember(Order = 1)]
    public float TimeExecute { get; set; }
    [DataMember(Order = 2)]
    public string GoalKey { get; set; }
    [DataMember(Order = 3)]
    public int GoalType { get; set; }
    
    [IgnoreConstructor]
    protected NitroxScheduledGoal()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
    
    public NitroxScheduledGoal(float timeExecute, string goalKey, int goalType)
    {
        TimeExecute = timeExecute;
        GoalKey = goalKey;
        GoalType = goalType;
    }

    public static NitroxScheduledGoal From(float timeExecute, string goalKey, int goalType)
    {
        return new NitroxScheduledGoal(timeExecute, goalKey, goalType);
    }

    public override string ToString()
    {
        return $"[NitroxScheduledGoal: TimeExecute: {TimeExecute}, GoalKey: {GoalKey}, GoalType: {GoalType}]";
    }
}
