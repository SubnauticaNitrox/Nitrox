using Harmony;
using NitroxServer.GameLogic.Monobehaviours;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class CellManager_HideEntities_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("HideEntities", new Type[] { typeof(Int3.Bounds), typeof(int) });

        public static bool Prefix()
        {
            return ChunkLoader.ALLOW_MAP_CLIPPING;
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
