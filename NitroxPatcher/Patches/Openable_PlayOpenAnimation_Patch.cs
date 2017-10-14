using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxClient.GameLogic.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class Openable_PlayOpenAnimation_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Openable);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("PlayOpenAnimation", BindingFlags.Public | BindingFlags.Instance);
        
        public static bool Prefix(Openable __instance, bool openState, float duration)
        {
            if (__instance.isOpen != openState)
            {
                String guid = GuidHelper.GetGuid(__instance.gameObject);
                Multiplayer.Logic.Interior.OpenableStateChanged(guid, openState, duration);
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
