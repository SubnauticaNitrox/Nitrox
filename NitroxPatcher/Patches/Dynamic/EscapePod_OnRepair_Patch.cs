using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class EscapePod_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.OnRepair());

        public static bool Prefix(EscapePod __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            Resolve<Entities>().BroadcastMetadataUpdate(id, new RepairedComponentMetadata(TechType.EscapePod.ToDto()));
            return true;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
