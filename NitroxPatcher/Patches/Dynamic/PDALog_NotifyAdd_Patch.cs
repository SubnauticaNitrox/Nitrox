using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDALog_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDALog);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyAdd", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Prefix(PDALog.Entry entry)
        {
            if (entry != null)
            {
                NitroxServiceLocator.LocateService<PDAManagerEntry>().LogAdd(entry);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
