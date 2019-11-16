using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Persistent
{
    class LargeWorldStreamer_LoadBatchObjectsThreaded_Patch : NitroxPatch
    {
        private static readonly MethodInfo TARGET_METHOD = typeof(LargeWorldStreamer).GetMethod("LoadBatchObjectsThreaded", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix()
        {
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
