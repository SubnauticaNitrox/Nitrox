using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class FireExtinguishers
{
    private readonly IPacketSender packetSender;

    private readonly Dictionary<NitroxId, FireExtinguisher> remotelyUsedExtinguishers = [];
    private readonly Dictionary<NitroxId, bool> localUsingState = [];

    public FireExtinguishers(IPacketSender packetSender)
    {
        this.packetSender = packetSender;

        // When a player stops holding a fire extinguisher, we'll stop it.
        // Even if we didn't receive a packet to notice that he's no longer using it,
        // we'll make sure that we mark it as not used.
        PlayerHeldItemChangedProcessor.OnHeldItemChanged += previousId =>
        {
            if (previousId.HasValue)
            {
                StopUsing(previousId.Value);
            }
        };
    }

    public void StartUsing(NitroxId id, FireExtinguisher fireExtinguisher)
    {
        remotelyUsedExtinguishers[id] = fireExtinguisher;
    }

    public void StopUsing(NitroxId id)
    {
        remotelyUsedExtinguishers.Remove(id);
    }

    public void BroadcastLocalState(NitroxId id, FireExtinguisher fireExtinguisher)
    {
        localUsingState.TryAdd(id, false);

        if (localUsingState[id] != fireExtinguisher.fxIsPlaying)
        {
            FireExtinguisherUsed packet = new(id, fireExtinguisher.fxIsPlaying);
            packetSender.Send(packet);
        }

        localUsingState[id] = fireExtinguisher.fxIsPlaying;
    }

    // Replacement for FireExtinguisher.Update() for extinguishers held by remote players
    // Code is mostly copied from FireExtinguisher.Update() and the various functions it calls
    public void RemoteUpdate(FireExtinguisher fireExtinguisher, RemotePlayerIdentifier identifier)
    {
        bool usedThisFrame = remotelyUsedExtinguishers.ContainsValue(fireExtinguisher);

        int num = identifier.RemotePlayer.AnimationController["is_underwater"] ? 1 : 0;
        if (num != fireExtinguisher.lastUnderwaterValue)
        {
            fireExtinguisher.lastUnderwaterValue = num;
            if (FMODUWE.IsInvalidParameterId(fireExtinguisher.fmodIndexInWater))
            {
                fireExtinguisher.fmodIndexInWater = fireExtinguisher.soundEmitter.GetParameterIndex("in_water");
            }
            fireExtinguisher.soundEmitter.SetParameterValue(fireExtinguisher.fmodIndexInWater, num);
        }

        Quaternion playerRotation = identifier.RemotePlayer.AnimationController.AimingRotation;

        fireExtinguisher.fireTarget = null;
        Vector3 vector = default;
        GameObject gameObject = null;

        // Set fireTarget for use in UpdateImpactFX()
        TraceRemotePlayerTargetPosition(identifier.transform.position, playerRotation, identifier.gameObject, 8f, ref gameObject, ref vector, true);
        if (gameObject)
        {
            Fire componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Fire>(gameObject);
            if (componentInHierarchy)
            {
                fireExtinguisher.fireTarget = componentInHierarchy;
            }
        }

        fireExtinguisher.fxControl.transform.eulerAngles = playerRotation.eulerAngles;

        if (usedThisFrame)
        {
            if (fireExtinguisher.fxControl && !fireExtinguisher.fxIsPlaying)
            {
                fireExtinguisher.fxControl.Play(0);
                fireExtinguisher.fxIsPlaying = true;
            }

            // TODO: Update to use 2D sound
            if (Vector3.Distance(Player.main.transform.position, fireExtinguisher.transform.position) < 30)
            {
                fireExtinguisher.soundEmitter.Play();
            }
        }
        else
        {
            fireExtinguisher.soundEmitter.Stop();
            if (fireExtinguisher.fxControl)
            {
                fireExtinguisher.fxControl.Stop(0);
                fireExtinguisher.fxIsPlaying = false;
            }
        }

        fireExtinguisher.UpdateImpactFX();
    }

    // Replacement for UWE.Utils.TraceFPSTargetPosition() that uses the remote player's view instead of the main camera
    private static bool TraceRemotePlayerTargetPosition(Vector3 playerPosition, Quaternion rotation, GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, bool includeUseableTriggers = true)
    {
        bool result = false;

        Vector3 position2 = playerPosition + new Vector3(0, -0.1f, 0);
        int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(position2, rotation * Vector3.forward), maxDist, -2097153);
        if (num == 0)
        {
            num = UWE.Utils.SpherecastIntoSharedBuffer(position2, 0.7f, rotation * Vector3.forward, maxDist, -2097153);
        }

        closestObj = null!;
        float num2 = 0f;
        for (int i = 0; i < num; i++)
        {
            RaycastHit raycastHit = UWE.Utils.sharedHitBuffer[i];
            if ((!(ignoreObj != null) || !UWE.Utils.IsAncestorOf(ignoreObj, raycastHit.collider.gameObject)) && (!raycastHit.collider || !raycastHit.collider.isTrigger || (includeUseableTriggers && raycastHit.collider.gameObject.layer == LayerMask.NameToLayer("Useable"))) && (closestObj == null || raycastHit.distance < num2))
            {
                closestObj = raycastHit.collider.gameObject;
                num2 = raycastHit.distance;
                position = raycastHit.point;
                result = true;
            }
        }

        return result;
    }
}
