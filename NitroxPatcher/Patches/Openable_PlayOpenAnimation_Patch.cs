using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
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
            Console.WriteLine(__instance.gameObject.name);
            Console.WriteLine(__instance.gameObject.transform.parent.gameObject.name);

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
