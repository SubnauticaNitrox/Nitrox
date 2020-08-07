using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class StarshipDoor_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(StarshipDoor).GetMethod(nameof(StarshipDoor.UnlockDoor), BindingFlags.Instance | BindingFlags.Public);

        public static void Prefix(StarshipDoor __instance)
        {
            if (__instance.doorLocked)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                StarshipDoorMetadata starshipDoorMetadata = new StarshipDoorMetadata(!__instance.doorLocked);
                Entities entities = NitroxServiceLocator.LocateService<Entities>();

                entities.BroadcastMetadataUpdate(id, starshipDoorMetadata);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
