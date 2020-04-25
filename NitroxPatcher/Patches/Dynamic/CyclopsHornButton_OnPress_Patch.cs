using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsHornButton_OnPress_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CyclopsHornButton).GetMethod("OnPress", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(CyclopsHornButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            NitroxServiceLocator.LocateService<Cyclops>().BroadcastActivateHorn(id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
