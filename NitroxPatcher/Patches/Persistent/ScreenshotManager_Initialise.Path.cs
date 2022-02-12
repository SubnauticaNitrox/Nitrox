using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    public class ScreenshotManager_Initialise : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => ScreenshotManager.Initialize(default(string)));

        public static void Prefix(ScreenshotManager __instance, ref string _savePath)
        {
            _savePath = Path.GetFullPath(NitroxUser.LauncherPath ?? ".");
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
