using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;
using static HandReticle;

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
        Log.InGame($"player died at {playerDeath.DeathPosition.X}, {playerDeath.DeathPosition.Y}, {playerDeath.DeathPosition.Z}");
        player.PlayerDeathEvent.Trigger(player);
        if (localPlayer.MarkDeathPointsWithBeacon)
        {
            CoroutineHost.StartCoroutine(SpawnDeathBeacon(playerDeath.DeathPosition, player.PlayerName));
        }
        // TODO: Add any death related triggers (i.e. scoreboard updates, rewards, etc.)
    }
    public static IEnumerator SpawnDeathBeacon(NitroxVector3 location, string playerName)
    {
        TaskResult<GameObject> result = new TaskResult<GameObject>();
        yield return CraftData.InstantiateFromPrefabAsync(TechType.Beacon, result, false);
        GameObject beacon = result.Get();
        if(beacon != null)
        {
            CrafterLogic.NotifyCraftEnd(beacon, TechType.Beacon);
            Pickupable item = beacon.GetComponent<Pickupable>();
            if(item != null)
            {
                item.Drop(location.ToUnity(), new Vector3(0,0,0), false);
                beacon.GetComponent<Beacon>().label = $"{playerName}'s death";
                CoroutineHost.StartCoroutine(TimedDestroyGameObject(beacon, 900));
            }
            else
            {
                Log.Error("Something went wrong in using the pickupable!");
            }
        }
        else
        {
            Log.Error("!!! something went wrong during beacon initialization");
        }
        yield break;
    }
    private static IEnumerator TimedDestroyGameObject(GameObject gameObj, float durationS)
    {
        yield return new WaitForSeconds(durationS);
        GameObject.Destroy(gameObj);
        yield break;
    }
}
