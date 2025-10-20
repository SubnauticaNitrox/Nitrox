using System.Runtime.Serialization;
using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

namespace Nitrox.Server.Subnautica.Models.GameLogic
{
    [Serializable]
    [DataContract]
    internal sealed class GameData
    {
        [DataMember(Order = 1)]
        public PdaStateData PDAState { get; set; }

        [DataMember(Order = 2)]
        public StoryGoalData StoryGoals { get; set; }

        [DataMember(Order = 3)]
        public StoryTimingData StoryTiming { get; set; }

        public static GameData From(PdaStateData pdaState, StoryGoalData storyGoals, ScheduleKeeper scheduleKeeper, StoryManager storyManager, TimeService timeService)
        {
            return new GameData
            {
                PDAState = pdaState,
                StoryGoals = StoryGoalData.From(storyGoals, scheduleKeeper),
                StoryTiming = StoryTimingData.From(storyManager, timeService)
            };
        }
    }
}
