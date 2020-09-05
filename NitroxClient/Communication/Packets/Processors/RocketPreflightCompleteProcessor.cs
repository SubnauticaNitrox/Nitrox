using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketPreflightCompleteProcessor : ClientPacketProcessor<RocketPreflightComplete>
    {
        public override void Process(RocketPreflightComplete packet)
        {
            GameObject gameObjectRocket = NitroxEntity.RequireObjectFrom(packet.Id);
            RocketPreflightCheckManager rocketPreflightCheckManager = gameObjectRocket.RequireComponentInChildren<RocketPreflightCheckManager>(true);

            rocketPreflightCheckManager.CompletePreflightCheck(packet.FlightCheck);
        }
    }
}

