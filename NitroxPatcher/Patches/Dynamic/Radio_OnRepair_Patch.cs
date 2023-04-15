using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    public class Radio_OnRepair_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.OnRepair());

        public static void Prefix(Radio __instance)
        {
            if (__instance.TryGetIdOrWarn(out NitroxId id))
            {
                Resolve<Entities>().BroadcastMetadataUpdate(id, new RepairedComponentMetadata(TechType.Radio.ToDto()));
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
