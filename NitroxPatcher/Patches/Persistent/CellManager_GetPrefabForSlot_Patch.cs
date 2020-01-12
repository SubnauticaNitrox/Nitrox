using System;
using System.Reflection;
using Harmony;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    class CellManager_GetPrefabForSlot_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("GetPrefabForSlot", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(IEntitySlot slot, out EntitySlot.Filler __result)
        {
            __result = default(EntitySlot.Filler);
            
            bool isMultiplayer = (Multiplayer.Main != null && Multiplayer.Main.IsMultiplayer());
            return !isMultiplayer;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
