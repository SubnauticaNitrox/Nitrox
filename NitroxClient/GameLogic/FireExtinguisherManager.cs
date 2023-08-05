using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
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
            FireExtinguisherUse packet = new(NitroxEntity.GetId(extinguisher.gameObject), extinguisher.fxIsPlaying);
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
            if (fireExtinguisher.fmodIndexInWater < 0)
            {
                fireExtinguisher.fmodIndexInWater = fireExtinguisher.soundEmitter.GetParameterIndex("in_water");
            }
            fireExtinguisher.soundEmitter.SetParameterValue(fireExtinguisher.fmodIndexInWater, num);
        }

        // Equivalent of UpdateTarget but with references to the remote player
        fireExtinguisher.fireTarget = null;
        Vector3 vector = default;
        GameObject gameObject = null;
        UWE.Utils.TraceFPSTargetPosition(identifier.gameObject, 8f, ref gameObject, ref vector, true);
        if (gameObject)
        {
            Fire componentInHierarchy = UWE.Utils.GetComponentInHierarchy<Fire>(gameObject);
            if (componentInHierarchy)
            {
                fireExtinguisher.fireTarget = componentInHierarchy;
            }
        }
        //

        if (usedThisFrame)
        {
            // Equivalent of FireExtinguisher.UseExtinguisher() but only the FX part
            if (fireExtinguisher.fxControl && !fireExtinguisher.fxIsPlaying)
            {
                fireExtinguisher.fxControl.Play(0);
                fireExtinguisher.fxIsPlaying = true;
            }
            //
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
}
