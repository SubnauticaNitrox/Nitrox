using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures
{
    [Serializable]
    [ProtoContract]
    public class FakeScheduledGoal
    {
        [ProtoMember(1)]
        public float TimeExecute { get; set; }
        [ProtoMember(2)]
        public string GoalKey { get; set; }
        [ProtoMember(3)]
        public string GoalType { get; set; }

        public static FakeScheduledGoal From(float timeExecute, string goalKey, string goalType)
        {
            return new FakeScheduledGoal
            {
                TimeExecute = timeExecute,
                GoalKey = goalKey,
                GoalType = goalType
            };
        }
    }
}
