using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    /*
     * Relays Cyclops FireSuppressionSystem to other players
     * This method was used instead of the OnClick to ensure, that the the suppression really started
     */
    public class CyclopsFireSuppressionButton_StartCooldown_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsFireSuppressionSystemButton).GetMethod("StartCooldown", BindingFlags.Public | BindingFlags.Instance);

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
