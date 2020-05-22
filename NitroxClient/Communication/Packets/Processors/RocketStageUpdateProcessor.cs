using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
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

            RocketConstructor rocketConstructor = gameObjectConstructor.GetComponent<RocketConstructor>();
            if (rocketConstructor)
            {
                GameObject gameObjectTobuild = SerializationHelper.GetGameObject(packet.SerializedGameObject);
                rocketConstructor.ReflectionCall("SendBuildBots", false, false, new object[] { gameObjectTobuild });
            }
        }
    }
}
