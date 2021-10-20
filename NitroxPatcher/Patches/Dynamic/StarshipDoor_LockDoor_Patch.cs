using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    class StarshipDoor_LockDoor_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StarshipDoor t) => t.LockDoor());

        public static void Prefix(StarshipDoor __instance)
        {
            if (!__instance.doorLocked)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                StarshipDoorMetadata starshipDoorMetadata = new(__instance.doorLocked, __instance.doorOpen);
                Entities entities = NitroxServiceLocator.LocateService<Entities>();

                entities.BroadcastMetadataUpdate(id, starshipDoorMetadata);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
