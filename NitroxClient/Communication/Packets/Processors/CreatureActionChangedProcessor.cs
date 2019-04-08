using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CreatureActionChangedProcessor : ClientPacketProcessor<CreatureActionChanged>
    {
        public static readonly Dictionary<NitroxId, CreatureAction> ActionByCreatureId = new Dictionary<NitroxId, CreatureAction>();

        public override void Process(CreatureActionChanged packet)
        {
            Optional<GameObject> opGameObject = NitroxIdentifier.GetObjectFrom(packet.Id);

            if (opGameObject.IsPresent())
            {
                CreatureAction action = packet.NewAction.GetCreatureAction(opGameObject.Get());
                ActionByCreatureId[packet.Id] = action;
            }
        }
    }
}
