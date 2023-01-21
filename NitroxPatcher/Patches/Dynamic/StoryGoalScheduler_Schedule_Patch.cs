using System.Linq;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic;

public class StoryGoalScheduler_Schedule_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalScheduler t) => t.Schedule(default));
    private static readonly IPacketSender packetSender = Resolve<IPacketSender>();

    // __state is true if the entry was already scheduled before, in which case we cancel the scheduling
    public static bool Prefix(StoryGoal goal, out bool __state)
    {
        __state = StoryGoalScheduler.main.schedule.Any(scheduledGoal => scheduledGoal.goalKey == goal.key) ||
                  (goal.goalType == Story.GoalType.Radio && StoryGoalManager.main.pendingRadioMessages.Contains(goal.key));

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
        packetSender.Send(new Schedule(timeExecute, goal.key, (int)goal.goalType));
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, true, true);
    }
}
