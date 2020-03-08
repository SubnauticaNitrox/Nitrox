using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class OnGoalUnlockTracker_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(OnGoalUnlockTracker);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyGoalComplete", BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(OnGoalUnlockData __instance, string completedGoal)
        {
            Dictionary<string, OnGoalUnlock> goalUnlocks = __instance.ReflectionGet("goalUnlocks", false, false) as Dictionary<string, OnGoalUnlock>;
            OnGoalUnlock onGoalUnlock;
            if (goalUnlocks.TryGetValue(completedGoal, out onGoalUnlock))
            {
                StoryEventSend packet = new StoryEventSend(StoryEventType.GoalUnlock, completedGoal);
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
