using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class LargeWorldOctreeCompiler_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LargeWorldOctreeCompiler);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RequestAbort", BindingFlags.Public | BindingFlags.Instance);

        public static void Postfix(LargeWorldOctreeCompiler __instance)
        {
            LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
            localPlayer.ClosingGame = true;
            Log.Info("Hola");
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
