using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerSyncTimeoutProcessor : ClientPacketProcessor<PlayerSyncTimeout>
{
    public override void Process(PlayerSyncTimeout packet)
    {
        Multiplayer.Main.StartCoroutine(TimeoutRoutine());
    }

    private IEnumerator TimeoutRoutine()
    {
        Log.InGame("Error: Initial sync timeout, closing game");
        yield return new WaitForSecondsRealtime(5);
        IngameMenu.main.QuitGame(false);
    }
}
