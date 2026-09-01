using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class WeldableWallPanelGeneric_UnlockDoor_Patch : NitroxPatch, IDynamicPatch
{
    private static Entities entities;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((WeldableWallPanelGeneric t) => t.UnlockDoor());

    public WeldableWallPanelGeneric_UnlockDoor_Patch(Entities e)
    {
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static void Postfix(WeldableWallPanelGeneric __instance)
    {
        if (__instance.liveMixin && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            WeldableWallPanelGenericMetadata weldableWallPanelGenericMetadata = new(__instance.liveMixin.health);
            entities.BroadcastMetadataUpdate(id, weldableWallPanelGenericMetadata);
        }
    }
}
