using System;
using System.Reflection;
using Harmony;
using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.Packets;
using Story;

namespace NitroxPatcher.Patches
{
    public class CrashedShipExploder_SetExplodeTime_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CrashedShipExploder);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SetExplodeTime", BindingFlags.NonPublic | BindingFlags.Instance);

        //set timeToStartCountdown and timeToStartWarning to an bigger value than what the server will have
        //so that the server explodes it first and overrides this value. See StoryEventHandler.ExplodeAurora
        public static void Postfix(CrashedShipExploder __instance)
        {
            __instance.timeToStartCountdown = 5 * 1200f * 1000f;
            __instance.timeToStartWarning = __instance.timeToStartCountdown - 1f;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
