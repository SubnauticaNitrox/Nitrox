using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Unlockables
{
    [ProtoContract]
    public class StoryGoalData
    {
        [ProtoMember(1)]
        public ThreadSafeCollection<string> CompletedGoals { get; } = new ThreadSafeCollection<string>();
        
        [ProtoMember(2)]
        public ThreadSafeCollection<string> RadioQueue { get; } = new ThreadSafeCollection<string>();
        
        [ProtoMember(3)]
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
