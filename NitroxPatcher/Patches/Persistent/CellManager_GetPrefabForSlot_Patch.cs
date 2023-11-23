using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    internal partial class CellManager_GetPrefabForSlot_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CellManager t) => t.GetPrefabForSlot(default(EntitySlot)));

        public static bool Prefix(IEntitySlot slot, out EntitySlot.Filler __result)
        {
            __result = default;
            return !Multiplayer.Active;
        }
    }
}
