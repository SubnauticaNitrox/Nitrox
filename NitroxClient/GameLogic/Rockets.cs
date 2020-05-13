using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Rockets
    {
        private readonly IPacketSender packetSender;
        private readonly Vehicles vehicles;

        public Rockets(IPacketSender packetSender, Vehicles vehicles)
        {
            this.packetSender = packetSender;
            this.vehicles = vehicles;
        }

        public void BroadcastElevatorActivation(NitroxId id, bool up)
        {
            using (packetSender.Suppress<RocketToggleElevator>())
            {
                vehicles.GetVehicles<NeptuneRocketModel>(id).ElevatorUp = up;
                RocketToggleElevator packet = new RocketToggleElevator(id, up);
                packetSender.Send(packet);
            }
        }

        public void BroadCastRocketStateUpdate(NitroxId id, TechType techType)
        {
            using (packetSender.Suppress<RocketStageUpdate>())
            {
                NeptuneRocketModel model = vehicles.GetVehicles<NeptuneRocketModel>(id);
                model.CurrentRocketStage += 1;
                RocketStageUpdate packet = new RocketStageUpdate(id, techType, model.CurrentRocketStage);
                packetSender.Send(packet);
            }
        }

        //TODO: Add more sync for end rocket
    }
}
