using System;
using System.Collections.Generic;
using NitroxModel.DataStructures;
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
