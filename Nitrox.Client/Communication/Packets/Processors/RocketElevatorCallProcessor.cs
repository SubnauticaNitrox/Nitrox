using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
