using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class PlayerPing : MonoBehaviour
{
    private const float DESPAWN_TIME = 10f;
    
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - spawnTime >= DESPAWN_TIME)
        {
            Destroy(gameObject);
        }
    }

    public static PlayerPing SpawnPlayerPing(NitroxVector3 location, string labelText, NitroxId pingId, Color playerColor)
    {
        GameObject pingObject = new($"Nitrox_PlayerPing_{labelText.Replace(" ", "")}_{pingId}");
        pingObject.transform.position = location.ToUnity();
        
        PlayerPing playerPing = pingObject.AddComponent<PlayerPing>();

        PingInstance signal = pingObject.AddComponent<PingInstance>();
        signal.pingType = PingType.Signal;
        signal.origin = pingObject.transform;
        signal.minDist = 5f;
        signal._label = labelText;
        signal.displayPingInManager = false;
        signal.visible = true;
        signal.Initialize();

        uGUI_Pings pings = FindObjectOfType<uGUI_Pings>();
        if (pings)
        {
            pings.OnColor(signal.Id, playerColor);
        }

        FMODEmitterController.PlayEventOneShot(PlayerPingManager.PING_OK_SOUND, 3, Player.main.transform.position);
        
        return playerPing;
    }
}
