using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
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

        public void BroadCastRocketStateUpdate(NitroxId id, NitroxId constructorId, TechType currentStageTech, UnityEngine.GameObject builtGameObject)
        {
            Optional<NeptuneRocketModel> model = vehicles.TryGetVehicle<NeptuneRocketModel>(id);

            if (model.HasValue)
            {
                model.Value.CurrentStage += 1;
                RocketStageUpdate packet = new RocketStageUpdate(id, constructorId, model.Value.CurrentStage, currentStageTech, SerializationHelper.GetBytes(builtGameObject));
                packetSender.Send(packet);
            }
            else
            {
                Log.Error($"Rockets: Can't find model for rocket with id {id} with constructor {constructorId} and currentStageTech {currentStageTech}");
            }
        }
    }
}
