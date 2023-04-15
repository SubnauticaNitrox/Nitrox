using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EscapePod_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.OnRepair());

        public static void Prefix(EscapePod __instance)
        {
            if (__instance.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Entities>().BroadcastMetadataUpdate(id, new RepairedComponentMetadata(TechType.EscapePod.ToDto()));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
