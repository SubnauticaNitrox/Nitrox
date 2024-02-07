using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LiveMixin_TakeDamage_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.TakeDamage(default(float), default(Vector3), default(DamageType), default(GameObject)));

    public static bool Prefix(out float __state, LiveMixin __instance, GameObject dealer)
    {
        // Persist the previous health value
        __state = __instance.health;

        if (!Resolve<LiveMixinManager>().IsWhitelistedUpdateType(__instance))
        {
            return true; // everyone should process this locally
        }

        return Resolve<LiveMixinManager>().ShouldApplyNextHealthUpdate(__instance, dealer);
    }

    public static void Postfix(float __state, LiveMixin __instance, bool __runOriginal)
    {
        // Did we realize a change in health?
        if (!__runOriginal || __state == __instance.health || Resolve<LiveMixinManager>().IsRemoteHealthChanging ||
            __instance.GetComponent<BaseCell>())
        {
            return;
        }

        // Let others know if we have a lock on this entity
        if (__instance.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(__instance.gameObject);

            if (metadata.HasValue)
            {
                Resolve<Entities>().BroadcastMetadataUpdate(id, metadata.Value);
            }
        }
    }
}
