using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class NitroxScheduledGoal
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual float TimeExecute { get; set; }
        [Index(1)]
        [ProtoMember(2)]
        public virtual string GoalKey { get; set; }
        [Index(2)]
        [ProtoMember(3)]
        public virtual string GoalType { get; set; }

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
