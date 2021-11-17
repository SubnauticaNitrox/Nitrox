using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class StoryGoalData
    {
        [JsonProperty, ProtoMember(1)]
        public ThreadSafeSet<string> CompletedGoals { get; } = new();

        [JsonProperty, ProtoMember(2)]
        public ThreadSafeList<string> RadioQueue { get; } = new();

        [JsonProperty, ProtoMember(3)]
        public ThreadSafeSet<string> GoalUnlocks { get; } = new();

        [JsonProperty, ProtoMember(4)]
        public ThreadSafeList<NitroxScheduledGoal> ScheduledGoals { get; } = new();

        public bool RemovedLatestRadioMessage()
        {
            if (RadioQueue.Count <= 0)
            {
                return false;
            }
            
            RadioQueue.RemoveAt(0);
            return true;
        }

        public InitialStoryGoalData GetInitialStoryGoalData(ScheduleKeeper scheduleKeeper)
        {
            return new InitialStoryGoalData(new List<string>(CompletedGoals), new List<string>(RadioQueue), new List<string>(GoalUnlocks), scheduleKeeper.GetScheduledGoals());
        }
    }
}
