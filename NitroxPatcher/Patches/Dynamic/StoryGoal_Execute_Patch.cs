using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using Story;

namespace NitroxPatcher.Patches.Dynamic;

public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => StoryGoal.Execute(default, default));

    /// <summary>
    /// Notifies the server of the execution of StoryGoals (except of PDA type)
    /// </summary>
    public static void Prefix(string key, Story.GoalType goalType, out bool __state)
    {
        __state = false;
        if (!Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        switch (goalType)
        {
            // Moved to Postfix
            case Story.GoalType.PDA:
                __state = true;
                // We make sure that no PDALogEntryAdd packet is sent when PDALog.Add will be called
                PDALog_Add_Patch.IgnoreKeys.Add(key);
                return;
            // When story goals are executed, if they were already completed before, they will do nothing.
            // We don't want to notify the server in this situation
            case Story.GoalType.Story:
                if (StoryGoalManager.main.completedGoals.Contains(key))
                {
                    return;
                }
                break;
        }
        Resolve<IPacketSender>().Send(new StoryGoalExecuted(key, goalType.ToDto()));
    }

    /// <summary>
    /// Notifies the server of the execution of StoryGoals of type PDA
    /// </summary>
    public static void Postfix(string key, Story.GoalType goalType, bool __state)
    {
        if (!__state || !PDALog.entries.TryGetValue(key, out PDALog.Entry entry))
        {
            return;
        }
        Resolve<IPacketSender>().Send(new StoryGoalExecuted(key, goalType.ToDto(), entry.timestamp));
    }

    public override void Patch(Harmony harmony)
    {
        PatchMultiple(harmony, TARGET_METHOD, prefix: true, postfix: true);
    }
}
