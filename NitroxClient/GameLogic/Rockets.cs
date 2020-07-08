using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.Packets;
using static Rocket;

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

        public void BroadcastRocketStateUpdate(NitroxId id, NitroxId constructorId, TechType currentStageTech, UnityEngine.GameObject builtGameObject)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.CurrentStage += 1;
                packetSender.Send(new RocketStageUpdate(id, constructorId, model.Value.CurrentStage, currentStageTech, SerializationHelper.GetBytes(builtGameObject)));
            }
            else
            {
                Log.Error($"{nameof(Rockets)}: Can't find model for rocket with id {id} with constructor {constructorId} and currentStageTech {currentStageTech}");
            }
        }

        //Called from the external panel
        public void CallElevator(NitroxId id, bool up)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.ElevatorUp = up;
                packetSender.Send(new RocketElevatorCall(id, up));
            }
            else
            {
                Log.Error($"{nameof(Rockets)}: Can't find model for rocket with id {id}");
            }
        }

        //Called from the inner panel
        public void CallElevatorControl(NitroxId id, RocketElevatorStates elevatorState)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                RocketElevatorControlCall packet = new RocketElevatorControlCall(id, elevatorState);
                model.Value.ElevatorUp = packet.Up;
                packetSender.Send(new RocketElevatorControlCall(id, elevatorState));
            }
            else
            {
                Log.Error($"{nameof(Rockets)}: Can't find model for rocket with id {id}");
            }
        }
    }
}
