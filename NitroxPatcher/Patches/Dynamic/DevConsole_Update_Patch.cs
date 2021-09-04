using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic
{
    class DevConsole_Update_Patch : NitroxPatch, IDynamicPatch
    {
        public static void Prefix()
        {
#if SUBNAUTICA
            DevConsole.disableConsole = NitroxConsole.DisableConsole;
#elif BELOWZERO
            PlatformUtils.SetDevToolsEnabled(!NitroxConsole.DisableConsole);
#endif
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, typeof(DevConsole).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic));
        }
    }
}
