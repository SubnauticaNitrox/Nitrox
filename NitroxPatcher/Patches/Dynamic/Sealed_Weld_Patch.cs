using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Sealed_Weld_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Sealed t) => t.Weld(default(float)));

        public static void Postfix(Sealed __instance)
        {
            if (__instance.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Entities>().EntityMetadataChanged(__instance, id);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
