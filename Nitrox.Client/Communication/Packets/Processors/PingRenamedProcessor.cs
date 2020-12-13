using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using UnityEngine;

namespace Nitrox.Client.Communication.Packets.Processors
{
    /// <summary>
    ///     Only syncs pings from beacons for now.
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
                // Not the object we're looking for.
                return;
            }
            Beacon beacon = obj.Value.GetComponent<Beacon>();
            if (!beacon)
            {
                // This can be ok if origin of ping instance component was not from a beacon (but from signal or other).
                return;
            }
            if (beacon.GetComponent<Player>())
            {
                // Skip over beacon component on player GameObjects
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
