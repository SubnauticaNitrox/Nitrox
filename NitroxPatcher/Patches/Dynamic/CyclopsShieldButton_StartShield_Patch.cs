using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class CyclopsShieldButton_StartShield_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsShieldButton t) => t.StartShield());

        public static void Postfix(CyclopsShieldButton __instance)
        {
            if (__instance.subRoot.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Cyclops>().BroadcastMetadataChange(id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
