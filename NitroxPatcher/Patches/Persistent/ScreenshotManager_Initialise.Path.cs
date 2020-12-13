﻿using System;
using System.IO;
using System.Reflection;
using Harmony;

namespace NitroxPatcher.Patches.Persistent
{
    public class ScreenshotManager_Initialise : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ScreenshotManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

        public static void Prefix(ScreenshotManager __instance, ref string _savePath)
        {
            _savePath = Path.GetFullPath(Environment.GetEnvironmentVariable("NITROX_LAUNCHER_PATH") ?? ".");
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
