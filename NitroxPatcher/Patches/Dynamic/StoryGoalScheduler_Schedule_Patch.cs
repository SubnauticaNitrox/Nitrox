using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StoryGoalScheduler_Schedule_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalScheduler t) => t.Schedule(default));

    // __state is true if the entry was already scheduled before, in which case we cancel the scheduling
    public static bool Prefix(StoryGoal goal, out bool __state)
    {
        __state = StoryGoalScheduler.main.schedule.Any(scheduledGoal => scheduledGoal.goalKey == goal.key) ||
                  (goal.goalType == Story.GoalType.Radio && StoryGoalManager.main.pendingRadioMessages.Contains(goal.key)) ||
                  StoryGoalManager.main.completedGoals.Contains(goal.key);

        if (__state)
        {
            Log.Debug($"Prevented a goal from being scheduled: {goal}");
        }
        else
        {
            Log.Debug($"A goal was scheduled: {goal}");
        }

        return !__state;
    }

    public static void Postfix(StoryGoal goal, bool __state)
    {
        if (__state || goal.delay == 0f || !Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        float timeExecute = StoryGoalScheduler.main.schedule.GetLast().timeExecute;
        Resolve<IPacketSender>().Send(new Schedule(timeExecute, goal.key, (int)goal.goalType));
    }
}
