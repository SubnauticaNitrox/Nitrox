using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class GoalManager_OnCompletedGoal_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GoalManager.main.OnCompleteGoal(default, default));

    public static void Prefix(string goalIdentifier, out bool __state)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            __state = false;
            return;
        }
        // Check to see if GoalManager already contains the goal.
        // __state is used to store whether it was a new goal or not.
        __state = !GoalManager.main.completedGoalNames.Contains(goalIdentifier);
    }

    public static void Postfix(string goalIdentifier, bool __state)
    {
        // Only send the completed goal if it is a new goal (__state==true)
        // and was successfully added to the completed goals
        if (__state && GoalManager.main.completedGoalNames.Contains(goalIdentifier))
        {
            Resolve<IPacketSender>().Send(new GoalCompleted(goalIdentifier, DayNightCycle.main.timePassedAsFloat));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
    }
}
