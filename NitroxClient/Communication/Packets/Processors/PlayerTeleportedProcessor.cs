using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.InitialSync;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.Communication.Packets.Processors
{
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
                    Vector3 position = (subRoot.transform.rotation * packet.DestinationTo.ToUnity()) + subRoot.transform.position;

                    Player.main.SetPosition(position);
                    Player.main.SetCurrentSub(subRoot);
                    return;
                }

                Player.main.SetCurrentSub(subRoot);
            }
            
            Player.main.SetPosition(packet.DestinationTo.ToUnity());
            // Freeze the player while he's loading its new position
            Player.main.cinematicModeActive = true;
            CoroutineHost.StartCoroutine(PlayerPositionInitialSyncProcessor.WaitForWorldLoad());
        }
    }
}
