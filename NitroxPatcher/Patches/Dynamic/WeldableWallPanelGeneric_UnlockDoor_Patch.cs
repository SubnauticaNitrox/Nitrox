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
    class WeldableWallPanelGeneric_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((WeldableWallPanelGeneric t) => t.UnlockDoor());

        public static void Postfix(WeldableWallPanelGeneric __instance)
        {
            if (__instance.liveMixin)
            {
                NitroxId id = NitroxEntity.GetId(__instance.gameObject);
                WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadata = new(__instance.liveMixin.health);
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
