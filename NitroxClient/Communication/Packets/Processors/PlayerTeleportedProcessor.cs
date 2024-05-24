using System;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel.Packets;
using UnityEngine;
using UWE;
using Terrain = NitroxClient.GameLogic.Terrain;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerTeleportedProcessor : ClientPacketProcessor<PlayerTeleported>
{
    public override void Process(PlayerTeleported packet)
    {
        Player.main.OnPlayerPositionCheat();

        if (packet.SubRootID.HasValue && NitroxEntity.TryGetComponentFrom(packet.SubRootID.Value, out SubRoot subRoot))
        {
            // Cyclops is using a local position system inside it's subroot
            if (subRoot.isCyclops)
            {
                // Reversing calculations from PlayerMovementBroadcaster.Update()
                Vector3 position = subRoot.transform.rotation * packet.DestinationTo.ToUnity() + subRoot.transform.position;

                Player.main.SetPosition(position);
                Player.main.SetCurrentSub(subRoot);
                return;
            }

            Player.main.SetCurrentSub(subRoot);
        }

        Vehicle currentVehicle = Player.main.currentMountedVehicle;
        if (currentVehicle)
        {
            currentVehicle.TeleportVehicle(packet.DestinationTo.ToUnity(), currentVehicle.transform.rotation);
            return;
        }

        Player.main.SetPosition(packet.DestinationTo.ToUnity());
        Player.main.cinematicModeActive = true;
        try
        {
            CoroutineHost.StartCoroutine(Terrain.WaitForWorldLoad());
        }
        catch (Exception e)
        {
            Player.main.cinematicModeActive = false;
            Log.Warn($"Something wrong happened while waiting for the terrain to load.\n{e}");
        }
    }
}
