using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;
using NitroxServer.Communication;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public Connection connection { get; private set; }
        private readonly HashSet<AbsoluteEntityCell> visibleCells = new HashSet<AbsoluteEntityCell>();

        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; }
        public ushort Id => PlayerContext.PlayerId;
        public string Name => PlayerContext.PlayerName;
        public Vector3 Position { get; set; }
        public Optional<string> SubRootGuid { get; set; }

        public Player(PlayerContext playerContext, Connection connection, Vector3 position, Optional<string> subRootGuid)
        {
            PlayerContext = playerContext;
            this.connection = connection;
            Position = position;
            SubRootGuid = subRootGuid;
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

        public override string ToString()
        {
            return Name;
        }

        public bool CanSee(Entity entity)
        {
            return (entity.ExistsInGlobalRoot || HasCellLoaded(entity.AbsoluteEntityCell));
        }

        public void SendPacket(Packet packet)
        {
            connection.SendPacket(packet);
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Player player = (Player)obj;

            return player.Id == Id;
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
