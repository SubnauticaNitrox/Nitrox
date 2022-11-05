using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Rockets
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;
        private readonly PlayerManager playerManager;

        public Rockets(IPacketSender packetSender, Vehicles vehicles, PlayerManager playerManager)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
            this.playerManager = playerManager;
        }

        /** Rocket states :
         * 0 : Launch Platform
         * 1 : Gantry
         * 2 : Boosters
         * 3 : Fuel Reserve
         * 4 : Cockpit
         * 5 : Final rocket
         **/
        public void BroadcastRocketStateUpdate(NitroxId id, TechType currentStageTech)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.CurrentStage += 1;

                //State 5 cannot be reached for the server based on players events, so we do it by hand
                if (model.Value.CurrentStage > 3)
                {
                    model.Value.CurrentStage = 5;
                }

                packetSender.SendIfGameCode(new RocketStageUpdate(id, model.Value.CurrentStage, currentStageTech));
            }
            else
            {
                Log.Error($"{nameof(Rockets.BroadcastRocketStateUpdate)}: Can't find model for rocket with id {id} and currentStageTech {currentStageTech}");
            }
        }

        public void CompletePreflightCheck(NitroxId id, PreflightCheck preflightCheck)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.PreflightChecks?.Add(preflightCheck);
                packetSender.SendIfGameCode(new RocketPreflightComplete(id, preflightCheck));
            }
            else
            {
                Log.Error($"{nameof(Rockets.CompletePreflightCheck)}: Can't find model for rocket with id {id}");
            }
        }

        public void CallElevator(NitroxId id, RocketElevatorPanel panel, bool up)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.ElevatorUp = up;
                packetSender.SendIfGameCode(new RocketElevatorCall(id, panel, up));
            }
            else
            {
                Log.Error($"{nameof(Rockets.CallElevator)}: Can't find model for rocket with id {id}");
            }
        }

        public void RequestRocketLaunch(Rocket rocket)
        {
            if (NitroxEntity.TryGetEntityFrom(rocket.gameObject, out NitroxEntity entity))
            {
                packetSender.SendIfGameCode(new RocketLaunch(entity.Id));
            }
            else
            {
                Log.Error($"{nameof(Rockets.RequestRocketLaunch)}: Can't find a NitroxEntity attached to the Rocket: {rocket.name}");
            }
        }

        public void RocketLaunch(NitroxId rocketId)
        {
            // Avoid useless calculations
            if (LaunchRocket.launchStarted)
            {
                return;
            }

            GameObject rocketObject = NitroxEntity.RequireObjectFrom(rocketId);
            GameObject sphereCenter = rocketObject.FindChild("AtmosphereVolume");
            LaunchRocket launchRocket = rocketObject.RequireComponentInChildren<LaunchRocket>(true);

            // Only launch if you're in the rocket so
            // verify if the distance to a centered point in the middle of the stage 3 of the rocket is inferior to 5.55 (pre-calculated radius)
            if (Player.main.IsUnderwater() ||
                Player.main.currentSub ||
                NitroxVector3.Distance(Player.main.transform.position.ToDto(), sphereCenter.transform.position.ToDto()) > 5.55f)
            {
                return;
            }

            // When the server sends this to the client, he should execute the rocket launch
            // Code extracted from LaunchRocket::OnHandClick
            LaunchRocket.SetLaunchStarted();
            PlayerTimeCapsule.main.Submit(null);
            launchRocket.StartCoroutine(launchRocket.StartEndCinematic());
            HandReticle.main.RequestCrosshairHide();

            // We also need to hide the other players
            foreach (RemotePlayer player in playerManager.GetAll())
            {
                player.PlayerModel.SetActive(false);
            }

            ErrorMessage.AddMessage(Language.main.Get("Nitrox_ThankForPlaying"));
        }
    }
}
