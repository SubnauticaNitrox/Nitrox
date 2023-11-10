using System.Collections;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
///     Makes sure the remote player object is loaded.
/// </summary>
/// <remarks>
///     This allows for the remote player to:<br/>
///      - use equipment
/// </remarks>
public class RemotePlayerInitialSyncProcessor : InitialSyncProcessor
{
    private readonly PlayerManager remotePlayerManager;

    public RemotePlayerInitialSyncProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int remotePlayersSynced = 0;

        foreach (PlayerContext otherPlayer in packet.OtherPlayers)
        {
            waitScreenItem.SetProgress(remotePlayersSynced, packet.OtherPlayers.Count);

            remotePlayerManager.Create(otherPlayer);

            remotePlayersSynced++;
            yield return null;
        }
    }
}
