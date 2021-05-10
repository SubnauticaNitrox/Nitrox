using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    /// See <see cref="SerializationHelper.GetBytesWithoutParent"/> for more info.
    public class UniqueIdentifier_Id_Getter_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo targetMethod = typeof(UniqueIdentifier).GetProperty(nameof(UniqueIdentifier.Id), BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();

        public static bool Prefix(string ___id, ref string __result)
        {
            if (___id == "SoundsLikeNitrox")
            {

                __result = null;
                return false;
            }

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, targetMethod);
        }
    }
}
