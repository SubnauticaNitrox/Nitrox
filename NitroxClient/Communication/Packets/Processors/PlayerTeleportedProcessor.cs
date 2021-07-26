using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayerTeleportedProcessor : ClientPacketProcessor<PlayerTeleported>
    {
        public override void Process(PlayerTeleported packet)
        {
            Player.main.OnPlayerPositionCheat();

            if (packet.SubRootID.HasValue)
            {
                if (NitroxEntity.TryGetComponentFrom(packet.SubRootID.Value, out SubRoot subRoot))
                {
                    //Reversing calculations from PlayerMovement.Update()
                    Vector3 position = subRoot.transform.rotation * packet.DestinationTo.ToUnity();
                    position += subRoot.transform.position;

                    Player.main.SetPosition(position);
                    Player.main.SetCurrentSub(subRoot);
                    return;
                }
            }

            Player.main.SetPosition(packet.DestinationTo.ToUnity());
        }
    }
}
