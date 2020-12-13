using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;
using Story;

namespace Nitrox.Patcher.Patches.Dynamic
{
    public class StoryGoal_Execute_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoal);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

        public static void Prefix(string key, Story.GoalType goalType)
        {
            if (!StoryGoalManager.main.completedGoals.Contains(key))
            {
                StoryEventSend packet = new StoryEventSend((StoryEventType) goalType, key);
                NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
