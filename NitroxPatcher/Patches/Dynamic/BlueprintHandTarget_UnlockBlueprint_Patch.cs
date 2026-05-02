using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Syncs DataBox (BlueprintHandTarget) usage across players.
/// When a player opens a DataBox to unlock a blueprint, this broadcasts the state to other players.
/// </summary>
public sealed partial class BlueprintHandTarget_UnlockBlueprint_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BlueprintHandTarget t) => t.UnlockBlueprint());

    public static void Postfix(BlueprintHandTarget __instance)
    {
        if (__instance.used && __instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().BroadcastMetadataUpdate(id, new BlueprintHandTargetMetadata(__instance.used));
        }
    }
}
