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
        if (!__runOriginal)
        {
            return;
        }

        // IsRemoteHealthChanging means we're replicating an action from the server and BaseCell is managed by BaseLeakManager
        if (Resolve<LiveMixinManager>().IsRemoteHealthChanging || __instance.GetComponent<BaseCell>())
        {
            return;
        }

        // PvP damage is always 0 so we need to check for it before the regular case
        if (HandlePvP(__instance, dealer, originalDamage))
        {
            return;
        }

        // At this point, if the victim didn't take damage, there's no point in broadcasting it
        if (__state != __instance.health)
        {
            return;
        }

        BroadcastDefaultTookDamage(__instance);
    }

    private static bool HandlePvP(LiveMixin liveMixin, GameObject dealer, float damage)
    {
        if (!liveMixin.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier))
        {
            return false;
        }

        // Dealer must be the local player, and we need to know about the item they're holding
        if (dealer != Player.mainObject || !Inventory.main.GetHeldObject())
        {
            return false;
        }
        
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
                return false;
        }

        Resolve<IPacketSender>().Send(new PvPAttack(remotePlayerIdentifier.RemotePlayer.PlayerId, damage, attackType));
        return true;
    }

    private static void BroadcastDefaultTookDamage(LiveMixin liveMixin)
    {
        // Let others know if we have a lock on this entity
        if (liveMixin.TryGetIdOrWarn(out NitroxId id) && Resolve<SimulationOwnership>().HasAnyLockType(id))
        {
            Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(liveMixin.gameObject);

            if (metadata.HasValue)
            {
                Resolve<Entities>().BroadcastMetadataUpdate(id, metadata.Value);
            }
        }
    }
}
