using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class PDALog_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDALog.NotifyAdd(default(PDALog.Entry)));

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
