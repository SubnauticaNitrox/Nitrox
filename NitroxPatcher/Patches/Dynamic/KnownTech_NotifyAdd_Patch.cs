using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic
{
    public class KnownTech_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => KnownTech.NotifyAdd(default(TechType), default(bool)));

        public static void Prefix(TechType techType, bool verbose)
        {
            Resolve<KnownTechEntry>().AddKnown(techType, verbose);
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
