using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class NitroxScheduledGoal
{
    [JsonMemberTransition]
    public float TimeExecute { get; set; }
    [JsonMemberTransition]
    public string GoalKey { get; set; }
    [JsonMemberTransition]
    public string GoalType { get; set; }

    public static NitroxScheduledGoal From(float timeExecute, string goalKey, string goalType)
    {
        return new NitroxScheduledGoal
        {
            TimeExecute = timeExecute,
            GoalKey = goalKey,
            GoalType = goalType
        };
    }

    public override string ToString()
    {
        return $"[NitroxScheduledGoal: TimeExecute: {TimeExecute}, GoalKey: {GoalKey}, GoalType: {GoalType}]";
    }
}
