using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_NotifyProgress_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAScanner);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static);
        private static PDAManagerEntry pdaManagerEntry = NitroxServiceLocator.LocateService<PDAManagerEntry>();

        public static void Prefix(PDAScanner.Entry entry)
        {
            if (entry != null)
            {
                pdaManagerEntry.Progress(entry, null);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
