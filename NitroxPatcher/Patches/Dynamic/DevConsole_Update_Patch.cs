using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic
{
    class DevConsole_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static void Prefix()
        {
            DevConsole.disableConsole = NitroxConsole.DisableConsole;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, typeof(DevConsole).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic));
        }
    }
}
