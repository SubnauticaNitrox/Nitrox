using NitroxClient.Communication.Packets.Processors.Abstract;
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
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);

            Rocket rocket = gameObjectRocket.RequireComponent<Rocket>();
            GameObject build = rocket.StartRocketConstruction();

            ItemGoalTracker.OnConstruct(packet.CurrentStageTech);

            RocketConstructor rocketConstructor = gameObjectRocket.RequireComponentInChildren<RocketConstructor>(true);
            rocketConstructor.SendBuildBots(build);
        }
    }
}
