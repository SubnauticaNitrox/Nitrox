using System;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    /// <summary>
    ///     Only synces pings from beacons for now.
    /// </summary>
    public class PingRenamedProcessor : ClientPacketProcessor<PingRenamed>
    {
        private readonly IPacketSender sender;

        public PingRenamedProcessor(IPacketSender sender)
        {
            this.sender = sender;
        }

        public override void Process(PingRenamed packet)
        {
            Optional<GameObject> obj = NitroxEntity.GetObjectFrom(packet.Id);
            if (!obj.HasValue)
            {
                throw new Exception($"Failed to find entity with Nitrox id '{packet.Id}'. It should also have a ping instance component.");
            }
            Beacon beacon = obj.Value.GetComponent<Beacon>();
            if (!beacon)
            {
                // This can be ok if origin of ping instance component was not from a beacon (but from signal or other).
                Log.Debug($"Skipped ping rename for non-beacon object with Nitrox id: '{packet.Id}', object name: '{obj.Value.GetFullName()}'");
                return;
            }

            using (sender.Suppress<PingRenamed>())
            {
                beacon.beaconLabel.SetLabel(packet.Name);
                Log.Debug($"Received ping rename: '{packet.Name}' on object '{obj.Value.GetFullName()}' with Nitrox id: '{packet.Id}'");
            }
        }
    }
}
