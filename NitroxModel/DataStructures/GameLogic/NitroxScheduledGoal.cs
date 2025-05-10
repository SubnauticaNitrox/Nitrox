using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.Networking.Packets;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable, DataContract]
public class NitroxScheduledGoal
{
    [DataMember(Order = 1)]
    public float TimeExecute { get; set; }
    [DataMember(Order = 2)]
    public string GoalKey { get; set; }
    [DataMember(Order = 3)]
    public Schedule.GoalCategory GoalCategory { get; set; }
    
    [IgnoreConstructor]
    protected NitroxScheduledGoal()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }
    
    public NitroxScheduledGoal(float timeExecute, string goalKey, Schedule.GoalCategory goalCategory)
    {
        TimeExecute = timeExecute;
        GoalKey = goalKey;
        GoalCategory = goalCategory;
    }

    public static NitroxScheduledGoal From(float timeExecute, string goalKey, Schedule.GoalCategory goalCategory)
    {
        return new NitroxScheduledGoal(timeExecute, goalKey, goalCategory);
    }

    public override string ToString()
    {
        return $"[NitroxScheduledGoal: TimeExecute: {TimeExecute}, GoalKey: {GoalKey}, GoalType: {GoalCategory}]";
    }
}
