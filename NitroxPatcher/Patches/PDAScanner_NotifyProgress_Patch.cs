using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches
{
    public class PDAScanner_NotifyProgress_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(PDAScanner);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("NotifyProgress", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Prefix(PDAScanner.Entry entry)
        {
            if (entry != null)
            {
<<<<<<< HEAD
<<<<<<< HEAD
                NitroxServiceLocator.LocateService<PDAManagerEntry>().Progress(entry);
=======
                NitroxServiceLocator.LocateService<PDAEntry>().Progress(entry);
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
                NitroxServiceLocator.LocateService<PDAManagerEntry>().Progress(entry);
>>>>>>> c7606c2... Changes Requested
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
