using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// <see cref="CyclopsExternalDamageManager.CreatePoint()"/> will eventually be overwritten and only executed if the client currently owns the Cyclops.
    /// </summary>
    class CyclopsExternalDamageManager_CreatePoint_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsExternalDamageManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CreatePoint", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(CyclopsExternalDamageManager __instance)
        {
            //Multiplayer.Logic.Cyclops.ExternalDamagePointCreated(__instance.GetComponentInParent<SubRoot>());
        }

        public override void Patch(HarmonyInstance harmony)
        {
            //PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
