using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using NitroxClient.GameLogic;
using System.Reflection;

namespace NitroxPatcher.Patches.Persistent
{
    public class FPSInputModule_ProcessMousePress_Patch : NitroxPatch
    {
        public static bool Prefix()
        {
            return !NitroxCursor.UnlockCursor;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, typeof(FPSInputModule).GetMethod("ProcessMousePress", BindingFlags.NonPublic | BindingFlags.Instance));
        }
    }
}
