using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;

namespace NitroxPatcher.Patches.Dynamic
{
    public class KnownTech_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(KnownTech).GetMethod("NotifyAdd", BindingFlags.NonPublic | BindingFlags.Static);

        public static void Prefix(TechType techType, bool verbose)
        {
            NitroxServiceLocator.LocateService<KnownTechEntry>().Add(techType, verbose);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
