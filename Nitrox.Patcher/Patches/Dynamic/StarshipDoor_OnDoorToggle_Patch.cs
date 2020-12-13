using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
{
    class StarshipDoor_OnDoorToggle_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(StarshipDoor).GetMethod("OnDoorToggle", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Postfix(StarshipDoor __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.gameObject);
            StarshipDoorMetadata starshipDoorMetadata = new StarshipDoorMetadata(__instance.doorLocked, __instance.doorOpen);
            Entities entities = NitroxServiceLocator.LocateService<Entities>();

            entities.BroadcastMetadataUpdate(id, starshipDoorMetadata);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
