using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketElevatorControlCallProcessor : ClientPacketProcessor<RocketElevatorControlCall>
    {
        public override void Process(RocketElevatorControlCall packet)
        {
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

            Rocket rocket = gameObjectRocket.RequireComponent<Rocket>();
            rocket.ElevatorControlButtonActivate();
        }
    }
}
