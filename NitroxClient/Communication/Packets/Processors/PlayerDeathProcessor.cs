using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerDeathProcessor : ClientPacketProcessor<PlayerDeathEvent>
{
    private readonly PlayerManager playerManager;
    private readonly LocalPlayer localPlayer;

    public PlayerDeathProcessor(PlayerManager playerManager, LocalPlayer localPlayer)
    {
        this.playerManager = playerManager;
        this.localPlayer = localPlayer;
    }

    public override void Process(PlayerDeathEvent playerDeath)
    {
        RemotePlayer player = Validate.IsPresent(playerManager.Find(playerDeath.PlayerId));
        Log.Debug($"{player.PlayerName} died");
        Log.InGame(Language.main.Get("Nitrox_PlayerDied").Replace("{PLAYER}", player.PlayerName));
        player.PlayerDeathEvent.Trigger(player);
        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
}

public class DeathBeacon : MonoBehaviour
{
    private static readonly float despawnDistance = 20f;

    public static IEnumerator SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        GameObject beacon = new();
        PingInstance signal = beacon.AddComponent<PingInstance>();
        signal.pingType = PingType.Signal;
        signal.displayPingInManager = true;
        signal.origin = beacon.transform;
        signal._label = $"{playerName}'s death";
        beacon.transform.position = location.ToUnity();
        beacon.AddComponent<DeathBeacon>();
        signal.Initialize();
        yield break;
    }

    private void Update()
    {
        if(Vector3.Distance(Player.main.transform.position, transform.position) <= despawnDistance)
        {
            Destroy(gameObject);
        }
    }
}
