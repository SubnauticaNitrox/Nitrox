using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CreatureActionChangedProcessor : ClientPacketProcessor<CreatureActionChanged>
    {
        public static readonly Dictionary<string, CreatureAction> ActionByGuid = new Dictionary<string, CreatureAction>();

        public override void Process(CreatureActionChanged packet)
        {
            Optional<GameObject> opGameObject = GuidHelper.GetObjectFrom(packet.Guid);

            if (opGameObject.IsPresent())
            {
                CreatureAction action = packet.NewAction.GetCreatureAction(opGameObject.Get());
                ActionByGuid[packet.Guid] = action;
            }
        }
    }
}
