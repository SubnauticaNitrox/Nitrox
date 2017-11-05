using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Tcp;
using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public string Id { get; }
        public Vector3 Position { get; set; }

        private readonly Connection connection;
        private readonly HashSet<VisibleCell> visibleCells = new HashSet<VisibleCell>();

        public Player(string id, Connection connection)
        {
            Id = id;
            this.connection = connection;
        }

        public void AddCells(IEnumerable<VisibleCell> cells)
        {
            lock (visibleCells)
            {
                foreach (VisibleCell cell in cells)
                {
                    visibleCells.Add(cell);
                }
            }
        }

        public void RemoveCells(IEnumerable<VisibleCell> cells)
        {
            lock (visibleCells)
            {
                foreach (VisibleCell cell in cells)
                {
                    visibleCells.Remove(cell);
                }
            }
        }

        public bool HasCellLoaded(VisibleCell cell)
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
