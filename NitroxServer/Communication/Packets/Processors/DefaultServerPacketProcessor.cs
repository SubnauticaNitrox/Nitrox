using System;
using System.Collections.Generic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    public class DefaultServerPacketProcessor : AuthenticatedPacketProcessor<Packet>
    {
        private readonly PlayerManager playerManager;

        // TODO: Use an Attribute and create this HashSet at runtime?
        private readonly HashSet<Type> loggingPacketBlackList = new HashSet<Type> {
            typeof(AnimationChangeEvent),
            typeof(Movement),
            typeof(VehicleMovement),
            typeof(ItemPosition),
            typeof(PlayerStats),
            typeof(VehicleColorChange)
        };

        public DefaultServerPacketProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(Packet packet, Player player)
        {
            if (!loggingPacketBlackList.Contains(packet.GetType()))
            {
                Log.Debug("Using default packet processor for: " + packet.ToString() + " and player " + player.Id);
            }

            // TODO: Enforce certain packets to have the correct simulation owning type.
            // Also use an IHasGuid or similar interface to make the checking-code generic. Use attributes to determine when a packet needs to be checked for ownership before sending.
            // Make a unittest to enforce all packets with that attribute to implement IHasGuid.

            // TODO: Decide how far we want to go with enforcement:
            // - Sender enforcement: This depends on the context. In case of exclusively entering/exiting a vehicle/pilotingseat, it's a requirement to claim control 'atomically' through the server.
            //   - In case of sending vehiclemovement packets, we can assume the enter-code has properly claimed exclusive access, so this is not a requirement.
            // - Server enforcement: Do not forward packets that require ownership when a player does not have it (prevents cheating and hacking etc), implementation mentioned above.
            // - Receiver enforcement: This would only be necessary if we don't trust the server, or expect the ownership 'database' to get out of sync, so that we can warn in time (only useful during development).

            playerManager.SendPacketToOtherPlayers(packet, player);
        }
    }
}
