using System;
using System.Runtime.Serialization;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic.Bases
{
    [Serializable]
    [DataContract]
    public class GameData
    {
        [DataMember(Order = 1)]
        public PDAStateData PDAState { get; set; }

        [DataMember(Order = 2)]
        public StoryGoalData StoryGoals { get; set; }

        [DataMember(Order = 3)]
        public StoryTimingData StoryTiming { get; set; }

        public static GameData From(PDAStateData pdaState, StoryGoalData storyGoals, ScheduleKeeper scheduleKeeper, StoryManager storyManager, TimeKeeper timeKeeper)
        {
            return new GameData
            {
                PDAState = pdaState,
                StoryGoals = StoryGoalData.From(storyGoals, scheduleKeeper),
                StoryTiming = StoryTimingData.From(storyManager, timeKeeper)
            };
        }
    }
}
