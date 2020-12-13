using System.Collections.Generic;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class CreatureActionChangedProcessor : ClientPacketProcessor<CreatureActionChanged>
    {
        public static readonly Dictionary<NitroxId, CreatureAction> ActionByCreatureId = new Dictionary<NitroxId, CreatureAction>();

        public override void Process(CreatureActionChanged packet)
        {
            Optional<GameObject> opGameObject = NitroxEntity.GetObjectFrom(packet.Id);
            if (opGameObject.HasValue)
            {
                CreatureAction action = packet.NewAction.GetCreatureAction(opGameObject.Value);
                ActionByCreatureId[packet.Id] = action;
            }
        }
    }
}
