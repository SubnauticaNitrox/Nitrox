using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class NitroxScheduledGoal
    {
        [ProtoMember(1)]
        public float TimeExecute { get; set; }
        [ProtoMember(2)]
        public string GoalKey { get; set; }
        [ProtoMember(3)]
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
}
