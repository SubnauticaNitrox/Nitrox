using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EscapePod_Awake_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.Awake());

        public static bool Prefix(EscapePod __instance)
        {
            return !EscapePodManager.SURPRESS_ESCAPE_POD_AWAKE_METHOD;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
