using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
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

    public static void Postfix(float __state, LiveMixin __instance, float originalDamage, GameObject dealer, bool __runOriginal)
    {
        bool healthChanged = __state != __instance.health;
        if (!__runOriginal || !ShouldBroadcastDamage(__instance, dealer, originalDamage, healthChanged))
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

    private static bool ShouldBroadcastDamage(LiveMixin victim, GameObject dealer, float damage, bool healthChanged)
    {
        if (Resolve<LiveMixinManager>().IsRemoteHealthChanging || victim.GetComponent<BaseCell>())
        {
            return false;
        }

        if (victim.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier))
        {
            // Handle it internally
            HandlePvP(remotePlayerIdentifier.RemotePlayer, dealer, damage);
            return false;
        }

        // The health change check must happen after the PvP one
        return healthChanged;
    }

    private static void HandlePvP(RemotePlayer remotePlayer, GameObject dealer, float damage)
    {
        if (dealer == Player.mainObject && Inventory.main.GetHeldObject())
        {
            PvPAttack.AttackType attackType;
            switch (Inventory.main.GetHeldTool())
            {
                case HeatBlade:
                    attackType = PvPAttack.AttackType.HeatbladeHit;
                    break;
                case Knife:
                    attackType = PvPAttack.AttackType.KnifeHit;
                    break;
                default:
                    // We don't want to send non-registered attacks
                    return;
            }
            Resolve<IPacketSender>().Send(new PvPAttack(remotePlayer.PlayerId, damage, attackType));
        }
    }
}
