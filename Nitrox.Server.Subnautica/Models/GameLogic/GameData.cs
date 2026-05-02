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

        public static GameData From(PdaManager pdaManager, StoryGoalData storyGoals, StoryScheduler storyScheduler, StoryManager storyManager, TimeService timeService)
        {
            return new GameData
            {
                PDAState = pdaManager.GetPdaStateCopy(),
                StoryGoals = StoryGoalData.From(storyGoals, storyScheduler),
                StoryTiming = StoryTimingData.From(storyManager, timeService)
            };
        }
    }
}
