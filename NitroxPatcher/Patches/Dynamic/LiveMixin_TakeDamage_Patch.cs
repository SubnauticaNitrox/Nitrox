using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LiveMixin_TakeDamage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.TakeDamage(default(float), default(Vector3), default(DamageType), default(GameObject)));

    public static bool Prefix(out float? __state, LiveMixin __instance, GameObject dealer)
    {
        __state = null;

        if (!Resolve<LiveMixinManager>().IsWhitelistedUpdateType(__instance))
        {
            return true; // everyone should process this locally
        }

        // Persist the previous health value
        __state = __instance.health;

        return Resolve<LiveMixinManager>().ShouldApplyNextHealthUpdate(__instance, dealer);
    }

    public static void Postfix(float? __state, LiveMixin __instance, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
    {
        // Did we realize a change in health?
        if (!__state.HasValue || __state.Value == __instance.health)
        {
            return;
        }

        // Let others know if we have a lock on this entity
        if (__instance.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Optional<EntityMetadata> metadata = EntityMetadataExtractor.Extract(__instance.gameObject);

            if (metadata.HasValue)
            {
                Resolve<Entities>().BroadcastMetadataUpdate(id, metadata.Value);
            }
        }
    }
}
