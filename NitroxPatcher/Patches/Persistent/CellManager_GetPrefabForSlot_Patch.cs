using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    internal class CellManager_GetPrefabForSlot_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CellManager t) => t.GetPrefabForSlot(default(EntitySlot)));

        public static bool Prefix(IEntitySlot slot, out EntitySlot.Filler __result)
        {
            __result = default;
            return !Multiplayer.Active;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
