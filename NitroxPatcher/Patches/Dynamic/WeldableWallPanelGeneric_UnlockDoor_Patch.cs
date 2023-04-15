using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class WeldableWallPanelGeneric_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((WeldableWallPanelGeneric t) => t.UnlockDoor());

    public static void Postfix(WeldableWallPanelGeneric __instance)
    {
        if (__instance.liveMixin && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadata = new(__instance.liveMixin.health);
            Resolve<Entities>().BroadcastMetadataUpdate(id, weldableWallPanelGenericMetadata);
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
