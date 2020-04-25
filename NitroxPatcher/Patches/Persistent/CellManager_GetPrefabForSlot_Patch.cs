using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    public class CellManager_GetPrefabForSlot_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(CellManager).GetMethod("GetPrefabForSlot", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(IEntitySlot slot, out EntitySlot.Filler __result)
        {
            __result = default(EntitySlot.Filler);
            return !Multiplayer.Active;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
