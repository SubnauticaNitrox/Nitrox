using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
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

        public void RequestRocketLaunch(Rocket rocket)
        {
            if (rocket.TryGetNitroxEntity(out NitroxEntity entity))
            {
                packetSender.Send(new RocketLaunch(entity.Id));
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

            Log.InGame(Language.main.Get("Nitrox_ThankForPlaying"));
        }
    }
}
