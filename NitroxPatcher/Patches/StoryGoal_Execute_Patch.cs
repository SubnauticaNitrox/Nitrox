using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using Story;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class StoryGoal_Execute_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(StoryGoal);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);

        public static void Prefix(string key, GoalType goalType)
        {
            StoryEventSend packet = new StoryEventSend((StoryEventType)goalType, key);
            Multiplayer.Logic.ClientBridge.Send(packet);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}