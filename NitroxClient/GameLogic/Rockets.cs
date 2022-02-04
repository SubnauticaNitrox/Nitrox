using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
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

        /** Rocket states :
         * 0 : Launch Platform
         * 1 : Gantry
         * 2 : Boosters
         * 3 : Fuel Reserve
         * 4 : Cockpit
         * 5 : Final rocket
         **/
        public void BroadcastRocketStateUpdate(NitroxId id, TechType currentStageTech)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.CurrentStage += 1;

                //State 5 cannot be reached for the server based on players events, so we do it by hand
                if (model.Value.CurrentStage > 3)
                {
                    model.Value.CurrentStage = 5;
                }

                packetSender.Send(new RocketStageUpdate(id, model.Value.CurrentStage, currentStageTech));
            }
            else
            {
                Log.Error($"{nameof(Rockets.BroadcastRocketStateUpdate)}: Can't find model for rocket with id {id} and currentStageTech {currentStageTech}");
            }
        }

        public void CompletePreflightCheck(NitroxId id, PreflightCheck preflightCheck)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.PreflightChecks?.Add(preflightCheck);
                packetSender.Send(new RocketPreflightComplete(id, preflightCheck));
            }
            else
            {
                Log.Error($"{nameof(Rockets.CompletePreflightCheck)}: Can't find model for rocket with id {id}");
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
                Log.Error($"{nameof(Rockets.CallElevator)}: Can't find model for rocket with id {id}");
            }
        }
    }
}
