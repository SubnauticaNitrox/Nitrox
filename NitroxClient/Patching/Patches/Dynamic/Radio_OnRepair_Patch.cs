using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class Radio_OnRepair_Patch : NitroxPatch, IDynamicPatch
{
    private static EntityMetadataManager entityMetadataManager;
    private static Entities entities;
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Radio t) => t.OnRepair());

    public Radio_OnRepair_Patch(EntityMetadataManager emm, Entities e)
    {
        entityMetadataManager = emm ?? throw new ArgumentNullException(nameof(emm));
        entities = e ?? throw new ArgumentNullException(nameof(e));
    }

    public static void Prefix(Radio __instance)
    {
        if (__instance.TryGetComponentInParent(out EscapePod pod) &&
            pod.TryGetIdOrWarn(out NitroxId id) &&
            entityMetadataManager.TryExtract(pod, out EntityMetadata metadata))
        {
            entities.BroadcastMetadataUpdate(id, metadata);
        }
    }
}
