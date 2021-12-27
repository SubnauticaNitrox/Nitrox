using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class GoalManager_OnCompletedGoal_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GoalManager.main.OnCompleteGoal(default, default));

    public static void Postfix(string goalIdentifier)
    {
        // Only send the completed goal if it was successfully added to the completed goals
        if (GoalManager.main.completedGoalNames.Contains(goalIdentifier))
        {
            Resolve<IPacketSender>().Send(new GoalCompleted(goalIdentifier));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
