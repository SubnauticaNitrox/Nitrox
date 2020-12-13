using System.Reflection;
using Harmony;
using Nitrox.Client.GameLogic;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Entities.Metadata;

namespace Nitrox.Patcher.Patches.Dynamic
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

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
