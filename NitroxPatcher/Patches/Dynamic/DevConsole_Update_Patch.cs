using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

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
            PatchPrefix(harmony, Reflect.Method((DevConsole t) => t.Update()));
        }
    }
}
