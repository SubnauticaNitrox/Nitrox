using Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.GameLogic;

internal sealed class GameData
{
    public PdaStateData PDAState { get; set; }

    public StoryGoalData StoryGoals { get; set; }

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
