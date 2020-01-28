using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using UnityEngine;
using NitroxModel.DataStructures.Util;
using NitroxServer.Communication.NetworkingLayer;
using NitroxModel.DataStructures;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public NitroxConnection connection { get; private set; }
        private readonly HashSet<AbsoluteEntityCell> visibleCells = new HashSet<AbsoluteEntityCell>();

        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; }
        public ushort Id => PlayerContext.PlayerId;
        public string Name => PlayerContext.PlayerName;
        public Vector3 Position { get; set; }
        public NitroxId GameObjectId { get; }
        public Optional<NitroxId> SubRootId { get; set; }

        public Player(PlayerContext playerContext, NitroxConnection connection, Vector3 position, Optional<NitroxId> subRootId)
        {
            PlayerContext = playerContext;
            this.connection = connection;
            Position = position;
            SubRootId = subRootId;
            GameObjectId = new NitroxId();
        }

        /// <summary>
        /// This overload is used when a NitroxId is already present
        /// </summary>
        /// <param name="playerContext"></param>
        /// <param name="connection"></param>
        /// <param name="position">the stored position</param>
        /// <param name="playerId">the stored playerId for the new player</param>
        /// <param name="subRootId"></param>
        public Player(PlayerContext playerContext, NitroxConnection connection, Vector3 position, NitroxId playerId, Optional<NitroxId> subRootId)
        {
            PlayerContext = playerContext;
            this.connection = connection;
            Position = position;
            SubRootId = subRootId;
            GameObjectId = playerId;
            //NitroxModel.Logger.Log.Info("Player ID: " + GameObjectId.ToString());
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
