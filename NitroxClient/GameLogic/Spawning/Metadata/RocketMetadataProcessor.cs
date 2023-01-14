using System.Linq;
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

        using(packetSender.Suppress<EntityMetadataUpdate>())
        {
            UpdateElevator(rocket, metadata);
            UpdateStage(rocket, metadata);
            UpdatePreflightChecks(rocket, metadata);
        }
    }

    private void UpdateElevator(Rocket rocket, RocketMetadata metadata)
    {
        // elevators will only be present on this model after the first construction event.
        if (rocket.currentRocketStage > 0)
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
        //CockpitSwitch and RocketPreflightCheckScreenElement are filled based on the RocketPreflightCheckManager
        if (rocket.currentRocketStage > 3)
        {
            RocketPreflightCheckManager rocketPreflightCheckManager = rocket.RequireComponent<RocketPreflightCheckManager>();
            rocketPreflightCheckManager.preflightChecks.AddRange(metadata.PreflightChecks.Select(i => (PreflightCheck)i));
        }
    }
}
