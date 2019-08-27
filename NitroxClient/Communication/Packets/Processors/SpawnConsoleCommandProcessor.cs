using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SpawnConsoleCommandProcessor : ClientPacketProcessor<SpawnConsoleCommandEvent>
    {
        private readonly IPacketSender packetSender;

        public SpawnConsoleCommandProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(SpawnConsoleCommandEvent packet)
        {
            GameObject gameObject = SerializationHelper.GetGameObject(packet.SerializeData);
            LargeWorldEntity.Register(gameObject);
            CrafterLogic.NotifyCraftEnd(gameObject, CraftData.GetTechType(gameObject));
            gameObject.SendMessage("StartConstruction", SendMessageOptions.DontRequireReceiver);
        }
    }
}
