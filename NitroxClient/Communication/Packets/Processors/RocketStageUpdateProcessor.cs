using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using Story;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketStageUpdateProcessor : ClientPacketProcessor<RocketStageUpdate>
    {
        public RocketStageUpdateProcessor()
        {

        }

        public override void Process(RocketStageUpdate packet)
        {
            GameObject gameObjectTobuild = SerializationHelper.GetGameObject(packet.SerializedGameObject);
            Optional<GameObject> gameObjectConstructor = NitroxEntity.GetObjectFrom(packet.ConstructorId);
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

            Rocket rocket = gameObjectRocket.RequireComponent<Rocket>();
            rocket.StartRocketConstruction();

            ItemGoalTracker.OnConstruct(packet.CurrentStageTech);

            if (gameObjectConstructor.HasValue)
            {
                Optional<RocketConstructor> opRocketConstructor = Optional.OfNullable(gameObjectRocket.GetComponent<RocketConstructor>());
                RocketConstructor rocketConstructor = opRocketConstructor.HasValue ? opRocketConstructor.Value : gameObjectRocket.RequireComponentInChildren<RocketConstructor>(true);
                rocketConstructor.ReflectionCall("SendBuildBots", false, false, new object[] { gameObjectTobuild });
            }
        }
    }
}
