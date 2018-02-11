using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Harmony;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent
{
    public class DevConsole_RegisterConsoleCommand_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(DevConsole);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RegisterConsoleCommand", BindingFlags.Public | BindingFlags.Static);

        public static void Prefix(Component originator, string command, bool caseSensitiveArgs, bool combineArgs)
        {
#pragma warning disable 618
            NitroxConsole.Main.AddCommandDirect("subnautica", command, () => DevConsole.SendConsoleCommand(command));
#pragma warning restore 618
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
