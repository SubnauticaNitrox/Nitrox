using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class DevConsoleEventEntry
    {
        private readonly IPacketSender packetSender;

        public DevConsoleEventEntry(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void Spawn(GameObject gameObject)
        {
            List<InteractiveChildObjectIdentifier> childIdentifiers = VehicleChildObjectIdentifierHelper.ExtractGuidsOfInteractiveChildren(gameObject);
            ConstructorBeginCrafting TPacket = new ConstructorBeginCrafting("", GuidHelper.GetGuid(gameObject), new NitroxModel.DataStructures.TechType(CraftData.GetTechType(gameObject).ToString()), 0, childIdentifiers, gameObject.transform.position, gameObject.transform.rotation, "", new Vector3[] { }, new Vector3[] { });
            VehicleModel VM = VehicleModelFactory.BuildFrom(TPacket);
            SpawnConsoleCommandEvent EntryChanged = new SpawnConsoleCommandEvent(SerializationHelper.GetBytes(gameObject), VM);
            packetSender.Send(EntryChanged);
        }
    }
}
