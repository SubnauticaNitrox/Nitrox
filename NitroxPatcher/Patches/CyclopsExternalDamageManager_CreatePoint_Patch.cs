using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Block from creating points if they aren't the owner of the sub
    /// </summary>
    class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsExternalDamageManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CreatePoint", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(CyclopsExternalDamageManager __instance, out bool __state)
        {
            __state = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(GuidHelper.GetGuid(__instance.subRoot.gameObject));

            return __state;
        }

        public static void Postfix(CyclopsExternalDamageManager __instance, bool __state)
        {
            if (__state)
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnCreatePoint(__instance.subRoot);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
