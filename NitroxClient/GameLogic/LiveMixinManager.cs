using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.GameLogic.FMOD;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class LiveMixinManager
{
    private readonly SimulationOwnership simulationOwnership;
    private static readonly HashSet<string> broadcastDeathClassIdWhitelist = new()
    {
        "7d307502-46b7-4f86-afb0-65fe8867f893" // Crash (fish)
    };

    public bool IsRemoteHealthChanging { get; private set; }

    /// <summary>
    ///     True while <see cref="ReplayRemoteKill"/> notifies an object's components of a death which happened for another player.
    /// </summary>
    public bool IsReplayingRemoteKill { get; private set; }

    public LiveMixinManager(SimulationOwnership simulationOwnership)
    {
        this.simulationOwnership = simulationOwnership;
    }

    // Currently, we only apply live mixin updates to vehicles as there is more work to implement
    // damage for regular entities like fish.
    public bool IsWhitelistedUpdateType(LiveMixin entity)
    {
        Vehicle vehicle = entity.GetComponent<Vehicle>();
        SubRoot subRoot = entity.GetComponent<SubRoot>();

        return (vehicle || (subRoot && subRoot.isCyclops));
    }
    
    public bool ShouldBroadcastDeath(LiveMixin liveMixin)
    {
        if (liveMixin.TryGetComponent(out UniqueIdentifier uniqueIdentifier) && !string.IsNullOrEmpty(uniqueIdentifier.classId))
        {
            return broadcastDeathClassIdWhitelist.Contains(uniqueIdentifier.classId);
        }
        
        return true;
    }

    /// <summary>
    ///     Sends the "OnKill" message which <see cref="LiveMixin.Kill"/> sent on the killer's side, for a death of which only
    ///     <see cref="CreatureDeath"/>'s part was replicated (see RemoveCreatureCorpseProcessor).
    ///     Without it, the other components (e.g. AI, animations, sounds) keep acting as if the object was alive.
    /// </summary>
    public void ReplayRemoteKill(LiveMixin liveMixin)
    {
        // Deaths broadcast with EntityDestroyed (see LiveMixin_Kill_Patch) are replicated by destroying the object
        if (liveMixin.destroyOnDeath || ShouldBroadcastDeath(liveMixin))
        {
            return;
        }

        GameObject gameObject = liveMixin.gameObject;
        // Entities which didn't die can be attached to the object (e.g. a grabbed vehicle or the local player),
        // in which case only the object's own components are notified
        bool notifyChildren = !(Player.main && Player.main.transform.IsChildOf(gameObject.transform)) &&
                              !gameObject.GetComponentInChildren<Vehicle>(true);

        IsReplayingRemoteKill = true;
        try
        {
            // The consequences of this death were already broadcast by the player for whom it happened
            using (PacketSuppressor<EntityDestroyed>.Suppress())
            using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
            using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
            using (FMODSystem.SuppressSendingSounds())
            {
                // The damage type isn't known, receivers without a parameter still get the message
                if (notifyChildren)
                {
                    gameObject.BroadcastMessage("OnKill", DamageType.Normal, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    gameObject.SendMessage("OnKill", DamageType.Normal, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
        finally
        {
            IsReplayingRemoteKill = false;
        }
    }

    public bool ShouldApplyNextHealthUpdate(LiveMixin receiver, GameObject dealer = null)
    {
        if (!receiver.TryGetNitroxId(out NitroxId id))
        {
            return false;
        }

        if (!simulationOwnership.HasAnyLockType(id) && !IsRemoteHealthChanging)
        {
            return false;
        }


        // Check to see if this health change is caused by docked vehicle collisions.  If so, we don't want to apply it.
        if (!dealer)
        {
            return true;
        }

        Vehicle dealerVehicle = dealer.GetComponent<Vehicle>();
        VehicleDockingBay vehicleDockingBay = receiver.GetComponentInChildren<VehicleDockingBay>();

        if (vehicleDockingBay && dealerVehicle)
        {
            if (vehicleDockingBay.GetDockedVehicle() == dealerVehicle ||
                vehicleDockingBay.interpolatingVehicle == dealerVehicle ||
                vehicleDockingBay.nearbyVehicle == dealerVehicle)
            {
                Log.Debug($"Dealer {dealer} is vehicle and currently docked or nearby {receiver}, do not harm it!");
                return false;
            }
        }

        return true;
    }

    public void SyncRemoteHealth(LiveMixin liveMixin, float remoteHealth, Vector3 position = default, DamageType damageType = DamageType.Normal)
    {
        if (liveMixin.health == remoteHealth)
        {
            return;
        }

        float difference = remoteHealth - liveMixin.health;

        IsRemoteHealthChanging = true;

        // We catch the exceptions here because we don't want IsRemoteHealthChanging to be stuck to true
        try
        {
            if (difference < 0)
            {
                liveMixin.TakeDamage(difference, position, damageType);
            }
            else
            {
                liveMixin.AddHealth(difference);
            }
        } catch (Exception e)
        {
            Log.Error(e, $"Encountered an expcetion while processing health update");
        }

        IsRemoteHealthChanging = false;

        // We mainly only do the above to trigger damage effects and sounds.  After those, we sync the remote value
        // to ensure that any floating point discrepencies aren't an issue.
        liveMixin.health = remoteHealth;
    }
}
