using System;
using System.Reflection;
using NitroxHarmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches
{
    public class StoryGoal_Execute_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoal);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

        public static void Prefix(string key, GoalType goalType)
        {
            StoryEventSend packet = new StoryEventSend((StoryEventType)goalType, key);
            NitroxServiceLocator.LocateService<IPacketSender>().Send(packet);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
