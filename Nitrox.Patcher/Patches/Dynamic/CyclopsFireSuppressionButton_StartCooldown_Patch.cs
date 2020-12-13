using System;
using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;

namespace Nitrox.Patcher.Patches.Dynamic
{
    /*
     * Relays Cyclops FireSuppressionSystem to other players
     * This method was used instead of the OnClick to ensure, that the the suppression really started
     */
    class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsFireSuppressionSystemButton);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("StartCooldown", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsFireSuppressionSystemButton __instance)
        {
            SubRoot cyclops = __instance.subRoot;
            NitroxId id = NitroxEntity.GetId(cyclops.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateFireSuppression(id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
