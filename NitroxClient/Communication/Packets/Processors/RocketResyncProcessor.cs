﻿using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class RocketResyncProcessor : ClientPacketProcessor<RocketResync>
{
    public override void Process(RocketResync packet)
    {
        GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.RocketId);

        gameObjectRocket.RequireComponentInChildren<RocketPreflightCheckManager>().preflightChecks.Clear();
        RocketPreflightCheckScreenElement[] screenElements = gameObjectRocket.GetComponentsInChildren<RocketPreflightCheckScreenElement>(true);
        CockpitSwitch[] cockpitSwitches = gameObjectRocket.GetComponentsInChildren<CockpitSwitch>(true);
        ThrowSwitch[] throwSwitches = gameObjectRocket.GetComponentsInChildren<ThrowSwitch>(true);

        foreach (RocketPreflightCheckScreenElement screenElement in screenElements)
        {
            if (packet.PreflightChecks.Contains(screenElement.preflightCheck))
            {
                screenElement.SetPreflightCheckComplete(screenElement.preflightCheck);
            }
            else
            {
                screenElement.SetPreflightCheckIncomplete(screenElement.preflightCheck);
            }
        }
        foreach (CockpitSwitch cockpit in cockpitSwitches)
        {
            bool completed = packet.PreflightChecks.Contains(cockpit.preflightCheck);
            // Reset cockpitSwitch state
            cockpit.collision.SetActive(true);
            cockpit.completed = false;
            cockpit.animator.SetBool("Completed", false);

            // Then enable if completed
            cockpit.completed = completed;
            cockpit.Start();
        }
        foreach (ThrowSwitch throwSwitch in throwSwitches)
        {
            bool completed = packet.PreflightChecks.Contains(throwSwitch.preflightCheck);
            // Reset throwSwitch state
            throwSwitch.completed = false;
            throwSwitch.triggerCollider.enabled = true;
            throwSwitch.cinematicTrigger.showIconOnHandHover = true;
            throwSwitch.lamp.GetComponent<SkinnedMeshRenderer>().material = throwSwitch.incompleteMat;

            // Then enable if completed
            throwSwitch.completed = completed;
            throwSwitch.Start();
        }
    }
}
