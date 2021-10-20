using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class OnGoalUnlockTracker_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(OnGoalUnlockTracker);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod(nameof(OnGoalUnlockTracker.NotifyGoalComplete), BindingFlags.Public | BindingFlags.Instance);

        private static IPacketSender packetSender;
        private static IPacketSender PacketSender => packetSender ??= NitroxServiceLocator.LocateService<IPacketSender>();

        public static void Prefix(OnGoalUnlockTracker __instance, string completedGoal)
        {
            if (__instance.goalUnlocks.ContainsKey(completedGoal))
            {
                StoryEventSend packet = new(StoryEventSend.EventType.GOAL_UNLOCK, completedGoal);
                PacketSender.Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
