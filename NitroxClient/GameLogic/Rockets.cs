using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
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
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                using (packetSender.Suppress<RocketStageUpdate>())
                {
                    model.Value.CurrentRocketStage += 1;
                    RocketStageUpdate packet = new RocketStageUpdate(id, techType, model.Value.CurrentRocketStage);
                    packetSender.Send(packet);
                }
            }
            else
            {
                Log.Error($"Rockets: Can't find model for rocket with id {id} and techtype {techType}");
            }
        }
    }

    //TODO: Add more sync for end rocket
}
