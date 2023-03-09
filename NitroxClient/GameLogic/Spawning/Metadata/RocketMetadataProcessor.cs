using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Helpers;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using UnityEngine;
using static Rocket;

namespace NitroxClient.GameLogic.Spawning.Metadata;

public class RocketMetadataProcessor : GenericEntityMetadataProcessor<RocketMetadata>
{
    // For newly connected players, we will only build the previous stage with construction bots for a certain time period.
    private const float MAX_ALLOWABLE_TIME_FOR_CONSTRUCTOR_BOTS = 10;

    private IPacketSender packetSender;

    public RocketMetadataProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
    }

    /** Rocket states :
     * 0 : Launch Platform
     * 1 : Gantry
     * 2 : Boosters
     * 3 : Fuel Reserve
     * 4 : Cockpit
     * 5 : Final rocket
     **/
    public override void ProcessMetadata(GameObject gameObject, RocketMetadata metadata)
    {
        Rocket rocket = gameObject.GetComponent<Rocket>();

        if (!rocket)
        {
            Log.Error($"Could not find Rocket on {gameObject.name}");
            return;
        }

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            UpdateElevator(rocket, metadata);
            UpdateStage(rocket, metadata);
            UpdatePreflightChecks(rocket, metadata);
        }
    }

    private void UpdateElevator(Rocket rocket, RocketMetadata metadata)
    {
        // elevators will only be present on this model after the gantry (p1) is built
        if (rocket.currentRocketStage > 1)
        {
            rocket.elevatorPosition = metadata.ElevatorPosition;
            rocket.elevatorState = (RocketElevatorStates)metadata.ElevatorState;
            rocket.SetElevatorPosition();
        }
    }

    private void UpdateStage(Rocket rocket, RocketMetadata metadata)
    {
        if (rocket.currentRocketStage == metadata.CurrentStage)
        {
            return;
        }

        bool allowConstructorBots = (DayNightCycle.main.timePassedAsFloat - metadata.LastStageTransitionTime) < MAX_ALLOWABLE_TIME_FOR_CONSTRUCTOR_BOTS;

        RocketConstructor rocketConstructor = rocket.RequireComponentInChildren<RocketConstructor>(true);

        for (int stage = rocket.currentRocketStage; stage < metadata.CurrentStage; stage++)
        {
            bool lastStageToBuild = (stage == (metadata.CurrentStage - 1));

            GameObject build = rocket.StartRocketConstruction();

            // We only want to use construction bots for the last constructed stage (just in case the client is out dated by multiple stages).
            // For all others, just force the completion of that stage.
            if (lastStageToBuild && allowConstructorBots)
            {
                rocketConstructor.SendBuildBots(build);
            }
            else
            {
                VFXConstructing vfxConstructing = build.GetComponent<VFXConstructing>();
                vfxConstructing.EndGracefully();
            }
        }
    }

    private void UpdatePreflightChecks(Rocket rocket, RocketMetadata metadata)
    {
        if (rocket.currentRocketStage < 4)
        {
            return;
        }

        IEnumerable<PreflightCheck> completedChecks = metadata.PreflightChecks.Select(i => (PreflightCheck)i);

        RocketPreflightCheckManager rocketPreflightCheckManager = rocket.RequireComponent<RocketPreflightCheckManager>();

        foreach(PreflightCheck completedCheck in completedChecks)
        {
            if (!rocketPreflightCheckManager.preflightChecks.Contains(completedCheck))
            {
                CompletePreflightCheck(rocket, completedCheck);
                rocketPreflightCheckManager.CompletePreflightCheck(completedCheck);
            }
        }
    }

    private void CompletePreflightCheck(Rocket rocket, PreflightCheck preflightCheck)
    {
        bool isCockpitCheck = (preflightCheck == PreflightCheck.LifeSupport ||
                               preflightCheck == PreflightCheck.PrimaryComputer);

        if (isCockpitCheck)
        {
            CompleteCockpitPreflightCheck(rocket, preflightCheck);
        }
        else
        {
            CompleteBasicPreflightCheck(rocket, preflightCheck);
        }
    }

    private void CompleteCockpitPreflightCheck(Rocket rocket, PreflightCheck preflightCheck)
    {
        CockpitSwitch[] cockpitSwitches = rocket.GetComponentsInChildren<CockpitSwitch>(true);

        foreach (CockpitSwitch cockpitSwitch in cockpitSwitches)
        {
            if (!cockpitSwitch.completed && cockpitSwitch.preflightCheck == preflightCheck)
            {
                cockpitSwitch.animator.SetBool("Completed", true);
                cockpitSwitch.completed = true;

                if (cockpitSwitch.collision)
                {
                    cockpitSwitch.collision.SetActive(false);
                }
            }
        }
    }

    private void CompleteBasicPreflightCheck(Rocket rocket, PreflightCheck preflightCheck)
    {
        ThrowSwitch[] throwSwitches = rocket.GetComponentsInChildren<ThrowSwitch>(true);

        foreach (ThrowSwitch throwSwitch in throwSwitches)
        {
            if (!throwSwitch.completed && throwSwitch.preflightCheck == preflightCheck)
            {
                throwSwitch.animator.AliveOrNull()?.SetTrigger("Throw");
                throwSwitch.completed = true;
                throwSwitch.cinematicTrigger.showIconOnHandHover = false;
                throwSwitch.triggerCollider.enabled = false;
                throwSwitch.lamp.GetComponent<SkinnedMeshRenderer>().material = throwSwitch.completeMat;
            }
        }
    }
}
