using System.Collections.Generic;
using System.Linq;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors
{
    /// <summary>
    /// Process who should win the race towards getting their damage point array validated as the winning one, and send the winner to all
    /// of the losers.
    /// </summary>
    class CyclopsDamageProcessor : AuthenticatedPacketProcessor<CyclopsDamage>
    {
        private readonly PlayerManager playerManager;

        /// <summary>
        /// Contains a list of all Cyclops that have been damaged. Undamaged Cyclops are not pre-loaded. It stores the Cyclops guid and damage point indexes.
        /// </summary>
        private static readonly Dictionary<string, CyclopsDamage> cyclopsDamagePoints = new Dictionary<string, CyclopsDamage>();

        public CyclopsDamageProcessor(PlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        public override void Process(CyclopsDamage packet, Player simulatingPlayer)
        {
            CyclopsDamage damage;

            if (cyclopsDamagePoints.TryGetValue(packet.Guid, out damage))
            {
                // If they're the same length, it means they probably have the same state, therefore they just need a call to know
                // if their activated damage points are the right ones. This call shouldn't go to others.
                if (packet.DamagePointIndexes.Length == damage.DamagePointIndexes.Length && packet.RoomFires.Length == damage.RoomFires.Length)
                {
                    simulatingPlayer.SendPacket(new CyclopsDamage(packet.Guid, packet.SubHealth, packet.DamageManagerHealth, packet.SubFireHealth, damage.DamagePointIndexes, damage.RoomFires));

                    Log.Debug("[CyclopsDamageProcessor PlayerId: " + simulatingPlayer.Id 
                        + " Guid: " + packet.Guid 
                        + " DamagePointIndexes: " + string.Join(", ", packet.DamagePointIndexes.Select(x => x.ToString()).ToArray())
                        + " Rooms on fire: " + packet.RoomFires.Length.ToString()
                        + " RoomFires: " + string.Join(", ", packet.RoomFires.Select(x => x.Room.ToString() + ": " + string.Join(", ", x.ActiveRoomFireNodes.Select(y => y.ToString()).ToArray())).ToArray())
                        + " Packet count matched, returning stored indexes: " + string.Join(", ", damage.DamagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");
                }
                else
                {
                    Log.Debug("[CyclopsDamageProcessor PlayerId: " + simulatingPlayer.Id 
                        + " Guid: " + packet.Guid 
                        + " DamagePointIndexes: " + string.Join(", ", packet.DamagePointIndexes.Select(x => x.ToString()).ToArray())
                        + " Rooms on fire: " + packet.RoomFires.Length.ToString()
                        + " RoomFires: " + string.Join(", ", packet.RoomFires.Select(x => x.Room.ToString() + ": " + string.Join(", ", x.ActiveRoomFireNodes.Select(y => y.ToString()).ToArray())).ToArray())
                        + " Packet count not the same. Replacing stored indexes: " + string.Join(", ", damage.DamagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");

                    // It's not synced with the server. This can either be by repairing a point or creating one. Their damage point state
                    // is now the winning one. Everyone needs to know what the new state is.
                    cyclopsDamagePoints[packet.Guid] = packet;
                    playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);
                }
            }
            else
            {
                // The player is really special, and gets to create their own damage addition to the Cyclops. Let's just hope it isn't from a de-synced guid.
                cyclopsDamagePoints.Add(packet.Guid, packet);
                playerManager.SendPacketToOtherPlayers(packet, simulatingPlayer);

                Log.Debug("[CyclopsDamageProcessor PlayerId: " + simulatingPlayer.Id 
                    + " Guid: " + packet.Guid 
                    + " DamagePointIndexes: " + string.Join(", ", packet.DamagePointIndexes.Select(x => x.ToString()).ToArray())
                    + " Rooms on fire: " + packet.RoomFires.Length.ToString()
                    + " RoomFires: " + string.Join(", ", packet.RoomFires.Select(x => x.Room.ToString() + ": " + string.Join(", ", x.ActiveRoomFireNodes.Select(y => y.ToString()).ToArray())).ToArray())
                    + " damage points for Cyclops not found. Creating new in Dictionary.");
            }
        }
    }
}
