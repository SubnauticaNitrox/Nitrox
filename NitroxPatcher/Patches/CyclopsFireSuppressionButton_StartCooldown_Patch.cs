using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches
{
    /*
     * Relays Cyclops FireSuppressionSystem to other players
     * This method was used instead of the OnClick to ensure, that the the suppression really started
     */
    class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsFireSuppressionSystemButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StartCooldown", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsFireSuppressionSystemButton __instance)
        {
            SubRoot cyclops = __instance.subRoot;
            string guid = GuidHelper.GetGuid(cyclops.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().ActivateFireSuppression(guid);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
