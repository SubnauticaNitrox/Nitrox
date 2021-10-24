using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDAScanner_NotifyProgress_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAScanner.NotifyProgress(default(PDAScanner.Entry)));

        public static void Prefix(PDAScanner.Entry entry)
        {
            if (entry != null)
            {
                Resolve<PDAManagerEntry>().Progress(entry, null);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
