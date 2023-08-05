using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic;

public class FireExtinguisherManager
{
    private readonly IPacketSender packetSender;

    private readonly Dictionary<NitroxId, FireExtinguisher> remotelyUsedFireExtinguishers = new();
    private readonly Dictionary<NitroxId, bool> latestRegisteredState = new();

    public FireExtinguisherManager(IPacketSender packetSender)
    {
        this.packetSender = packetSender;

        // When a player stops holding a fire extinguisher, we'll stop it. Even if we didn't receive a packet to notice that he's no longer using it, we'll make sure that we mark it as not used.
        PlayerHeldItemChangedProcessor.OnHeldItemChanged += (previousId) =>
        {
            if (previousId.HasValue)
            {
                remotelyUsedFireExtinguishers.Remove(previousId.Value);
            }
        };
    }

    public void StartUsing(NitroxId extinguisherId, FireExtinguisher fireExtinguisher)
    {
        if (!remotelyUsedFireExtinguishers.ContainsKey(extinguisherId))
        {
            remotelyUsedFireExtinguishers.Add(extinguisherId, fireExtinguisher);
        }
    }

    public void StopUsing(NitroxId extinguisherId)
    {
        remotelyUsedFireExtinguishers.Remove(extinguisherId);
    }

    public void BroadcastFireExtinguisherState(NitroxId extinguisherId, FireExtinguisher extinguisher)
    {
        if (!latestRegisteredState.TryGetValue(extinguisherId, out bool previousValue) || previousValue != extinguisher.fxIsPlaying)
        {
            FireExtinguisherUse packet = new(extinguisherId, extinguisher.fxIsPlaying);
            packetSender.Send(packet);
        }
        latestRegisteredState[extinguisherId] = extinguisher.fxIsPlaying;
    }

    public void RemotelyUseFireExtinguisher(FireExtinguisher fireExtinguisher, RemotePlayerIdentifier identifier)
    {
        // Code mainly copied from FireExtinguisher.Update() but adapted to only keep the visual part
        bool usedThisFrame = remotelyUsedFireExtinguishers.ContainsValue(fireExtinguisher);

        int num = identifier.RemotePlayer.AnimationController["is_underwater"] ? 1 : 0;
        if (num != fireExtinguisher.lastUnderwaterValue)
        {
            fireExtinguisher.lastUnderwaterValue = num;
            if (FMODUWE.IsInvalidParameterId(fireExtinguisher.fmodIndexInWater))
            {
                fireExtinguisher.fmodIndexInWater = fireExtinguisher.soundEmitter.GetParameterIndex("in_water");
            }
            // TODO: Only play sound if close enough
            fireExtinguisher.soundEmitter.SetParameterValue(fireExtinguisher.fmodIndexInWater, num);
        }

        // Equivalent of UpdateTarget but with references to the remote player
        fireExtinguisher.fireTarget = null;
        Vector3 vector = default;
        GameObject gameObject = null;
        
        TraceRemotePlayerTargetPosition(identifier.gameObject, 8f, ref gameObject, ref vector, true);
        if (gameObject)
        {
            Fire componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Fire>(gameObject);
            if (componentInHierarchy)
            {
                fireExtinguisher.fireTarget = componentInHierarchy;
            }
        }
        else
        {
            // Added line to make sure that the FX is in the right direction
            fireExtinguisher.fxControl.transform.eulerAngles = identifier.RemotePlayer.AnimationController.AimingRotation.eulerAngles;
        }
        // end of UpdateTarget

        if (usedThisFrame)
        {
            // Equivalent of FireExtinguisher.UseExtinguisher() but only the FX part
            if (fireExtinguisher.fxControl && !fireExtinguisher.fxIsPlaying)
            {
                fireExtinguisher.fxControl.Play(0);
                fireExtinguisher.fxIsPlaying = true;
            }
            // end of UseExtinguisher
            fireExtinguisher.soundEmitter.Play();
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

    /// <summary>
    /// Replacement for <see cref="UWE.Utils.TraceFPSTargetPosition(GameObject, float, ref GameObject, ref Vector3, out Vector3, bool)"/>
    /// because it originally uses the main camera while we want to use the remote player's view
    /// </summary>
    private static bool TraceRemotePlayerTargetPosition(GameObject ignoreObj, float maxDist, ref GameObject closestObj, ref Vector3 position, bool includeUseableTriggers = true)
    {
        // TODO: Make it so that the camera reference is replaced by the remote player's stuff
        bool result = false;
        Camera camera = MainCamera.camera;
        Vector3 position2 = camera.transform.position;
        int num = UWE.Utils.RaycastIntoSharedBuffer(new Ray(position2, camera.transform.forward), maxDist, -2097153);
        if (num == 0)
        {
            num = UWE.Utils.SpherecastIntoSharedBuffer(position2, 0.7f, camera.transform.forward, maxDist, -2097153);
        }

        closestObj = null;
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
