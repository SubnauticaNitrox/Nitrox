using System.Collections.Generic;
using Newtonsoft.Json;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using ProtoBufNet;

namespace Nitrox.Server.GameLogic.Unlockables
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class StoryGoalData
    {
        [JsonProperty, ProtoMember(1)]
        public ThreadSafeCollection<string> CompletedGoals { get; } = new ThreadSafeCollection<string>();

        [JsonProperty, ProtoMember(2)]
        public ThreadSafeCollection<string> RadioQueue { get; } = new ThreadSafeCollection<string>();

        [JsonProperty, ProtoMember(3)]
        public ThreadSafeCollection<string> GoalUnlocks { get; } = new ThreadSafeCollection<string>();

        public void RemovedLatestRadioMessage()
        {
            RadioQueue.RemoveAt(0);
        }

        public InitialStoryGoalData GetInitialStoryGoalData()
        {
            return new InitialStoryGoalData(new List<string>(CompletedGoals), new List<string>(RadioQueue), new List<string>(GoalUnlocks));
        }
    }
}
