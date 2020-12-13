using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class StarshipDoor_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(StarshipDoor).GetMethod(nameof(StarshipDoor.UnlockDoor), BindingFlags.Instance | BindingFlags.Public);

        public static void Prefix(StarshipDoor __instance)
        {
            if (__instance.doorLocked)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                StarshipDoorMetadata starshipDoorMetadata = new StarshipDoorMetadata(!__instance.doorLocked, __instance.doorOpen);
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
