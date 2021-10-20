using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches.Dynamic
{
    public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoal);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod(nameof(StoryGoal.Execute), BindingFlags.Public | BindingFlags.Static);

        private static IPacketSender packetSender;
        private static IPacketSender PacketSender => packetSender ??= NitroxServiceLocator.LocateService<IPacketSender>();
        
        public static void Prefix(string key, Story.GoalType goalType)
        {
            if (!StoryGoalManager.main.completedGoals.Contains(key))
            {
                StoryEventSend packet = new((StoryEventSend.EventType)goalType, key);
                PacketSender.Send(packet);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
