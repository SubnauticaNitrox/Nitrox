using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketElevatorCallProcessor : ClientPacketProcessor<RocketElevatorCall>
    {
        public override void Process(RocketElevatorCall packet)
        {
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);
            Rocket rocket = gameObjectRocket.RequireComponent<Rocket>();

            if (packet.Panel == RocketElevatorPanel.EXTERNAL_PANEL)
            {
                rocket.CallElevator(packet.Up);
            }
            else if (packet.Panel == RocketElevatorPanel.INTERNAL_PANEL)
            {
                rocket.ElevatorControlButtonActivate();
            }
        }
    }
}
