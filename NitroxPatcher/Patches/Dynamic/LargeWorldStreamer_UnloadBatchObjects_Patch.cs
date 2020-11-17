using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class LargeWorldStreamer_UnloadBatchObjects_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LargeWorldStreamer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UnloadBatchObjects", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool Prefix(Int3 index)
        {
            NitroxServiceLocator.LocateService<Terrain>().CellUnloaded(index);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
