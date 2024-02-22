using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LiveMixin_AddHealth_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.AddHealth(default));

    /// <summary>
    /// We can broadcast packet update when we aren't processing remote health change
    /// </summary>
    private static bool CanBroadcast => !Resolve<LiveMixinManager>().IsRemoteHealthChanging;

    public static bool Prefix(out float __state, LiveMixin __instance)
    {
        // Persist the previous health value
        __state = __instance.health;

        if (!Resolve<LiveMixinManager>().IsWhitelistedUpdateType(__instance))
        {
            return true; // everyone should process this locally
        }

        return Resolve<LiveMixinManager>().ShouldApplyNextHealthUpdate(__instance);
    }

    public static void Postfix(float __state, LiveMixin __instance, bool __runOriginal)
    {
        if (!__runOriginal || __state == __instance.health)
        {
            return;
        }

        // This foreach will detect the presence of some components which need to be handled specifically
        foreach (MonoBehaviour monoBehaviour in __instance.GetComponents<MonoBehaviour>())
        {
            switch (monoBehaviour)
            {
                case RadiationLeak radiationLeak:
                    HandleRadiationLeakRepair(radiationLeak);
                    return;
                case BaseCell baseCell:
                    HandleBaseLeakRepair(baseCell, __instance);
                    return;
            }
        }
        // If the foreach ends, it'll mean that none of those components will have been detected so we just execute the generic case
        HandleGenericEntity(__instance);
    }

    private static void HandleRadiationLeakRepair(RadiationLeak radiationLeak)
    {
        if (!CanBroadcast || !radiationLeak.TryGetNitroxId(out NitroxId leakId))
        {
            return;
        }
        
        Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(radiationLeak);
        if (metadata.HasValue)
        {
            Resolve<Entities>().BroadcastMetadataUpdate(leakId, metadata.Value);
        }
    }

    private static void HandleBaseLeakRepair(BaseCell baseCell, LiveMixin liveMixin)
    {
        if (liveMixin.IsFullHealth())
        {
            if (liveMixin.TryGetComponentInParent(out BaseLeakManager baseLeakManager, true))
            {
                LeakRepaired leakRepaired = baseLeakManager.RemoveLeakByAbsoluteCell(baseCell.cell);
                if (CanBroadcast && leakRepaired != null)
                {
                    Resolve<IPacketSender>().Send(leakRepaired);
                }
            }
        }
        else if (liveMixin.TryGetComponentInParent(out BaseHullStrength baseHullStrength, true))
        {
            BaseHullStrength_CrushDamageUpdate_Patch.BroadcastChange(baseHullStrength, liveMixin);
        }
    }

    private static void HandleGenericEntity(LiveMixin victim)
    {
        // Let others know if we have a lock on this entity
        if (!CanBroadcast || !victim.TryGetIdOrWarn(out NitroxId id) || !Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            return;
        }

        Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(victim.gameObject);
        if (metadata.HasValue)
        {
            Resolve<Entities>().BroadcastMetadataUpdate(id, metadata.Value);
        }
    }
}
