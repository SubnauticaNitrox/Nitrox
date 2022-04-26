using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class KnownTech_NotifyAnalyze_Patch : NitroxPatch, IDynamicPatch
    {
        
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => KnownTech.NotifyAnalyze(default(KnownTech.AnalysisTech), default(bool)));

        public static void Prefix(KnownTech.AnalysisTech analysis, bool verbose)
        {
            Resolve<KnownTechEntry>().AddAnalyzed(analysis.techType, verbose);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
