using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Syncs DataBox (BlueprintHandTarget) usage across players.
/// When a player opens a DataBox to unlock a blueprint, this broadcasts the state to other players.
/// </summary>
internal sealed partial class BlueprintHandTarget_UnlockBlueprint_Patch : NitroxPatch, IDynamicPatch
{
    private static Entities entities;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BlueprintHandTarget t) => t.UnlockBlueprint());

    public BlueprintHandTarget_UnlockBlueprint_Patch(Entities e)
    {
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static void Postfix(BlueprintHandTarget __instance)
    {
        if (__instance.used && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            entities.BroadcastMetadataUpdate(id, new BlueprintHandTargetMetadata(__instance.used));
        }
    }
}
