using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class GoalManager_OnCompletedGoal_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GoalManager.main.OnCompleteGoal(default, default));

    private static bool isNewGoal;

    public static void Prefix(string goalIdentifier)
    {
        // Check to see if GoalManager already contains the goal
        isNewGoal = !GoalManager.main.completedGoalNames.Contains(goalIdentifier);
    }

    public static void Postfix(string goalIdentifier)
    {
        // Only send the completed goal if it is a new goal and was successfully added to the completed goals
        if (isNewGoal && GoalManager.main.completedGoalNames.Contains(goalIdentifier))
        {
            Resolve<IPacketSender>().Send(new GoalCompleted(goalIdentifier));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
