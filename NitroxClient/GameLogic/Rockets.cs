using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;

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

        public void BroadcastRocketStateUpdate(NitroxId id, TechType currentStageTech)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.CurrentStage += 1;
                packetSender.Send(new RocketStageUpdate(id, model.Value.CurrentStage, currentStageTech));
            }
            else
            {
                Log.Error($"{nameof(Rockets)}: Can't find model for rocket with id {id} and currentStageTech {currentStageTech}");
            }
        }

        public void CallElevator(NitroxId id, RocketElevatorPanel panel, bool up)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.ElevatorUp = up;
                packetSender.Send(new RocketElevatorCall(id, panel, up));
            }
            else
            {
                Log.Error($"{nameof(Rockets)}: Can't find model for rocket with id {id}");
            }
        }
    }
}
