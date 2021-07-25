﻿using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly ThreadSafeCollection<EquippedItemData> equippedItems;
        private readonly ThreadSafeCollection<EquippedItemData> modules;
        private readonly ThreadSafeCollection<AbsoluteEntityCell> visibleCells;

        public ThreadSafeCollection<NitroxTechType> UsedItems { get; }
        public ThreadSafeCollection<string> QuickSlotsBinding { get; set; }

        public NitroxConnection Connection { get; set; }
        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; }
        public string Name { get; }
        public bool IsPermaDeath { get; set; }
        public NitroxVector3 Position { get; set; }
        public NitroxId GameObjectId { get; }
        public Optional<NitroxId> SubRootId { get; set; }
        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }
        public NitroxVector3? LastStoredPosition { get; set; }
        public Optional<NitroxId> LastStoredSubRootID { get; set; }

        public Player(ushort id, string name, bool isPermaDeath, PlayerContext playerContext, NitroxConnection connection,
                      NitroxVector3 position, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats,
                      IEnumerable<NitroxTechType> usedItems, IEnumerable<string> quickSlotsBinding,
                      IEnumerable<EquippedItemData> equippedItems, IEnumerable<EquippedItemData> modules)
        {
            Id = id;
            Name = name;
            IsPermaDeath = isPermaDeath;
            PlayerContext = playerContext;
            Connection = connection;
            Position = position;
            SubRootId = subRootId;
            GameObjectId = playerId;
            Permissions = perms;
            Stats = stats;
            LastStoredPosition = null;
            LastStoredSubRootID = Optional.Empty;
            UsedItems = new ThreadSafeCollection<NitroxTechType>(usedItems);
            QuickSlotsBinding = new ThreadSafeCollection<string>(quickSlotsBinding);
            this.equippedItems = new ThreadSafeCollection<EquippedItemData>(equippedItems);
            this.modules = new ThreadSafeCollection<EquippedItemData>(modules);
            visibleCells = new ThreadSafeCollection<AbsoluteEntityCell>(new HashSet<AbsoluteEntityCell>(), false);
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

        public bool CanSee(Entity entity)
        {
            return entity.ExistsInGlobalRoot || HasCellLoaded(entity.AbsoluteEntityCell);
        }

        public void SendPacket(Packet packet)
        {
            Connection.SendPacket(packet);
        }

        public void Teleport(NitroxVector3 destination, Optional<NitroxId> subRootID)
        {
            PlayerTeleported playerTeleported = new PlayerTeleported(Name, Position, destination, subRootID);

            Position = playerTeleported.DestinationTo;
            LastStoredPosition = playerTeleported.DestinationFrom;
            LastStoredSubRootID = subRootID;
            SendPacket(playerTeleported);
        }

        public override string ToString()
        {
            return $"[Player - Id: {Id}, Name: {Name}, Perms: {Permissions}, Position: {Position}]";
        }

        protected bool Equals(Player other)
        {
            return Id == other.Id;
        }
    }
}
