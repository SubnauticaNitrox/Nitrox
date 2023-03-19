using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class LargeWorldStreamer_UnloadBatch_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((LargeWorldStreamer t) => t.UnloadBatch(default(Int3)));

        public static bool Prefix(Int3 index)
        {
            NitroxServiceLocator.LocateService<Terrain>().BatchUnloaded(index);

            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
