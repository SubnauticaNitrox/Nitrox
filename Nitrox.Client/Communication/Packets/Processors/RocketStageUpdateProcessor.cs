using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.Packets;
using Story;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
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
            rocketConstructor.ReflectionCall("SendBuildBots", false, false, build);
        }
    }
}
