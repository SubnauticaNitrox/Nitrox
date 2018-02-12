using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
 using NitroxServer.GameLogic;
 using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public string Id { get; }
        public string Name { get; }
        public PlayerSettings PlayerSettings;
        public Vector3 Position { get; set; }

        private readonly Connection connection;
        private readonly HashSet<AbsoluteEntityCell> visibleCells = new HashSet<AbsoluteEntityCell>();
        
        public Player(PlayerContext playerContext, Connection connection)
        {
            Id = playerContext.PlayerId;
            Name = playerContext.PlayerName;
            PlayerSettings = playerContext.PlayerSettings;
            this.connection = connection;
        }

        public void AddCells(IEnumerable<AbsoluteEntityCell> cells)
        {
            lock (visibleCells)
            {
                foreach (AbsoluteEntityCell cell in cells)
                {
                    visibleCells.Add(cell);
                }
            }
        }

        public void RemoveCells(IEnumerable<AbsoluteEntityCell> cells)
        {
            lock (visibleCells)
            {
                foreach (AbsoluteEntityCell cell in cells)
                {
                    visibleCells.Remove(cell);
                }
            }
        }

        public bool HasCellLoaded(AbsoluteEntityCell cell)
        {
            lock (visibleCells)
            {
                return visibleCells.Contains(cell);
            }
        }

        public void SendPacket(Packet packet)
        {
            if (connection.Open)
            {
                connection.SendPacket(packet, null);
            }
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Player player = (Player)obj;

            return (player.Id == Id);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 269;
                hash = hash * 23 + Id.GetHashCode();
                return hash;
            }
        }
    }
}
