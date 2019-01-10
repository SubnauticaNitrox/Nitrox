using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches
{
    class DevConsole_Update_Patch : NitroxPatch
    {
        public static void Prefix()
        {
            DevConsole.disableConsole = NitroxConsole.DisableConsole;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, typeof(DevConsole).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic));
        }
    }
}
