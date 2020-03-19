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
        public NitroxConnection connection { get; set; }
        private readonly HashSet<AbsoluteEntityCell> visibleCells = new HashSet<AbsoluteEntityCell>();
        private readonly List<EquippedItemData> equippedItems = new List<EquippedItemData>();
        private readonly List<EquippedItemData> modules = new List<EquippedItemData>();

        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public NitroxId GameObjectId { get; }
        public Optional<NitroxId> SubRootId { get; set; }
        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }
        
        public Player(ushort id, string name, PlayerContext playerContext, NitroxConnection connection, Vector3 position, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats, List<EquippedItemData> equippedItems, List<EquippedItemData> modules)
        {
            Id = id;
            Name = name;
            PlayerContext = playerContext;
            this.connection = connection;
            Position = position;
            SubRootId = subRootId;
            GameObjectId = playerId;
            Permissions = perms;
            Stats = stats;
            this.equippedItems = equippedItems;
            this.modules = modules;
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

        public void AddModule(EquippedItemData module)
        {
            lock (modules)
            {
                modules.Add(module);
            }
        }

        public void RemoveModule(NitroxId id)
        {
            lock (modules)
            {
                modules.RemoveAll(item => item.ItemId == id);
            }
        }

        public List<EquippedItemData> getAllModules()
        {
            lock (modules)
            {
                return new List<EquippedItemData>(modules);
            }
        }

        public void AddEquipment(EquippedItemData equipment)
        {
            lock (equippedItems)
            {
                equippedItems.Add(equipment);
            }
        }

        public void RemoveEquipment(NitroxId id)
        {
            lock (equippedItems)
            {
                equippedItems.RemoveAll(item => item.ItemId == id);
            }
        }

        public List<EquippedItemData> getAllEquipment()
        {
            lock (equippedItems)
            {
                return new List<EquippedItemData>(equippedItems);
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
