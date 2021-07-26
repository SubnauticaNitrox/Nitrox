using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic
{
    class WeldableWallPanelGeneric_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = typeof(WeldableWallPanelGeneric).GetMethod(nameof(WeldableWallPanelGeneric.UnlockDoor), BindingFlags.Instance | BindingFlags.Public);

        public static void Postfix(WeldableWallPanelGeneric __instance)
        {
            if (__instance.liveMixin)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadata = new WeldableWallPanelGenericMetadata(__instance.liveMixin.health);
                Entities entities = NitroxServiceLocator.LocateService<Entities>();

                entities.BroadcastMetadataUpdate(id, weldableWallPanelGenericMetadata);
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
