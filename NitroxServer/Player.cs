using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.NetworkingLayer;
using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly ThreadSafeCollection<EquippedItemData> equippedItems;
        private readonly ThreadSafeCollection<EquippedItemData> modules;
        private readonly ThreadSafeCollection<AbsoluteEntityCell> visibleCells = new ThreadSafeCollection<AbsoluteEntityCell>(new HashSet<AbsoluteEntityCell>(), false);
        public NitroxConnection connection { get; set; }

        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public NitroxId GameObjectId { get; }
        public Optional<NitroxId> SubRootId { get; set; }
        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }

        public Player(ushort id, string name, PlayerContext playerContext, NitroxConnection connection, Vector3 position, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats, IEnumerable<EquippedItemData> equippedItems,
                      IEnumerable<EquippedItemData> modules)
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
            this.equippedItems = new ThreadSafeCollection<EquippedItemData>(equippedItems);
            this.modules = new ThreadSafeCollection<EquippedItemData>(modules);
        }

        public static bool operator ==(Player left, Player right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Player left, Player right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Player)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public void AddCells(IEnumerable<AbsoluteEntityCell> cells)
        {
            foreach (AbsoluteEntityCell cell in cells)
            {
                visibleCells.Add(cell);
            }
        }

        public void RemoveCells(IEnumerable<AbsoluteEntityCell> cells)
        {
            foreach (AbsoluteEntityCell cell in cells)
            {
                visibleCells.Remove(cell);
            }
        }

        public bool HasCellLoaded(AbsoluteEntityCell cell)
        {
            return visibleCells.Contains(cell);
        }

        public void AddModule(EquippedItemData module)
        {
            modules.Add(module);
        }

        public void RemoveModule(NitroxId id)
        {
            modules.RemoveAll(item => item.ItemId == id);
        }

        public List<EquippedItemData> GetModules()
        {
            return modules.ToList();
        }

        public void AddEquipment(EquippedItemData equipment)
        {
            equippedItems.Add(equipment);
        }

        public void RemoveEquipment(NitroxId id)
        {
            equippedItems.RemoveAll(item => item.ItemId == id);
        }

        public List<EquippedItemData> GetEquipment()
        {
            return equippedItems.ToList();
        }

        public override string ToString()
        {
            return $"[Player {{{nameof(Id)}: {Id}}}, {{{nameof(Name)}: {Name}}}]";
        }

        public bool CanSee(Entity entity)
        {
            return entity.ExistsInGlobalRoot || HasCellLoaded(entity.AbsoluteEntityCell);
        }

        public void SendPacket(Packet packet)
        {
            connection.SendPacket(packet);
        }

        protected bool Equals(Player other)
        {
            return Id == other.Id;
        }
    }
}
