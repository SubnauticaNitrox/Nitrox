using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UWE;
using Terrain = NitroxClient.GameLogic.Terrain;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerTeleportedProcessor : ClientPacketProcessor<PlayerTeleported>
{
    public override void Process(PlayerTeleported packet)
    {
        Player.main.OnPlayerPositionCheat();

        Vehicle currentVehicle = Player.main.currentMountedVehicle;
        if (currentVehicle)
        {
            currentVehicle.TeleportVehicle(packet.DestinationTo.ToUnity(), currentVehicle.transform.rotation);
            Player.main.WaitForTeleportation();
            return;
        }

        Player.main.SetPosition(packet.DestinationTo.ToUnity());
        
        if (packet.SubRootID.HasValue && NitroxEntity.TryGetComponentFrom(packet.SubRootID.Value, out SubRoot subRoot))
        {
            Player.main.SetCurrentSub(subRoot, true);
            return;
        }
        
        // Freeze the player while it's loading its new position
        Player.main.cinematicModeActive = true;
        Player.main.WaitForTeleportation();

        CoroutineHost.StartCoroutine(Terrain.WaitForWorldLoad().OnYieldError(e =>
        {
            Player.main.cinematicModeActive = false;
            Log.Warn($"Something wrong happened while waiting for the terrain to load.\n{e}");
        }));
    }
}
