using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class RocketLaunchProcessor : ClientPacketProcessor<RocketLaunch>
{
    private readonly PlayerManager playerManager;

    public RocketLaunchProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(RocketLaunch rocketLaunch)
    {
        GameObject rocketObject = NitroxEntity.RequireObjectFrom(rocketLaunch.RocketId);
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
        if (LaunchRocket.launchStarted)
        {
            return;
        }
        LaunchRocket.SetLaunchStarted();
        PlayerTimeCapsule.main.Submit(null);
        launchRocket.StartCoroutine(launchRocket.StartEndCinematic());
        HandReticle.main.RequestCrosshairHide();

        // We also need to hide the other players
        foreach (RemotePlayer player in playerManager.GetAll())
        {
            player.PlayerModel.SetActive(false);
        }
    }
}
