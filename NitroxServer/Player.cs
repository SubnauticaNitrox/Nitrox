using System.Collections.Generic;
using NitroxModel.DataStructures;
using UnityEngine;
using NitroxModel.Tcp;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public string Id { get; }
        public Vector3 Position { get; set; }

        private readonly Connection connection;
        private readonly HashSet<VisibleCell> visibleCells;

        public Player(string id, Connection connection)
        {
            this.Id = id;
            this.visibleCells = new HashSet<VisibleCell>();
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
    }
}
