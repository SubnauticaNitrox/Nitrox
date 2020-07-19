using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using Story;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketStageUpdateProcessor : ClientPacketProcessor<RocketStageUpdate>
    {
        public override void Process(RocketStageUpdate packet)
        {
            GameObject gameObjectConstructor = NitroxEntity.RequireObjectFrom(packet.ConstructorId);
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

            Rocket rocket = gameObjectRocket.RequireComponent<Rocket>();
            rocket.StartRocketConstruction();

            ItemGoalTracker.OnConstruct(packet.CurrentStageTech);

            RocketConstructor rocketConstructor = gameObjectConstructor.GetComponentInChildren<RocketConstructor>(true);
            if (rocketConstructor)
            {
                GameObject gameObjectTobuild = SerializationHelper.GetGameObject(packet.SerializedGameObject);
                rocketConstructor.ReflectionCall("SendBuildBots", false, false, gameObjectTobuild);
            }
            else
            {
                Log.Error($"{nameof(RocketStageUpdateProcessor)}: Can't find attached rocketconstructor with id {packet.ConstructorId} for rocket with id {packet.Id}");
            }
        }
    }
}
