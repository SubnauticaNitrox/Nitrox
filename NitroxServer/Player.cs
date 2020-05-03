using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.NetworkingLayer;
using DTO = NitroxModel.DataStructures;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly DTO.ThreadSafeCollection<EquippedItemData> equippedItems;
        private readonly DTO.ThreadSafeCollection<EquippedItemData> modules;
        private readonly DTO.ThreadSafeCollection<AbsoluteEntityCell> visibleCells = new DTO.ThreadSafeCollection<AbsoluteEntityCell>(new HashSet<AbsoluteEntityCell>(), false);

        public Player(ushort id, string name, bool isPermaDeath, PlayerContext playerContext, NitroxConnection connection, DTO.Vector3 position, DTO.NitroxId playerId, Optional<DTO.NitroxId> subRootId, Perms perms, PlayerStatsData stats,
                      IEnumerable<EquippedItemData> equippedItems,
                      IEnumerable<EquippedItemData> modules)
        {
            Id = id;
            Name = name;
            IsPermaDeath = isPermaDeath;
            PlayerContext = playerContext;
            this.connection = connection;
            Position = position;
            SubRootId = subRootId;
            GameObjectId = playerId;
            Permissions = perms;
            Stats = stats;
            this.equippedItems = new DTO.ThreadSafeCollection<EquippedItemData>(equippedItems);
            this.modules = new DTO.ThreadSafeCollection<EquippedItemData>(modules);
        }

        public NitroxConnection connection { get; set; }

        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; }
        public string Name { get; set; }
        public bool IsPermaDeath { get; set; }
        public DTO.Vector3 Position { get; set; }
        public DTO.NitroxId GameObjectId { get; }
        public Optional<DTO.NitroxId> SubRootId { get; set; }
        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }

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

        public void RemoveModule(DTO.NitroxId id)
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

        public void RemoveEquipment(DTO.NitroxId id)
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
