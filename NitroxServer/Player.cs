using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication.NetworkingLayer;
using NitroxServer.GameLogic;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly ThreadSafeCollection<EquippedItemData> equippedItems;
        private readonly ThreadSafeCollection<EquippedItemData> modules;
        private readonly ThreadSafeCollection<AbsoluteEntityCell> visibleCells;
        
        public NitroxConnection connection { get; set; }
        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; }
        public string Name { get; set; }
        public bool IsPermaDeath { get; set; }
        public NitroxTransform Transform { get; private set; }
        public NitroxId GameObjectId => Transform.Id;
        public Optional<NitroxId> SubRootId { get; set; }

        private Task playerBackgroundThread;

        public CellChanges ProcessCellChanges()
        {
            CellChanges cellChanges = new CellChanges();
            NitroxVector3 b = new NitroxVector3(0f, -8f, 0f);
            NitroxTransform transform = new NitroxTransform(new NitroxVector3(-2048, -3040, -2048), new NitroxQuaternion(0, 0, 0, 1), NitroxVector3.One, null);
            CellManager.UpdateCenter((NitroxInt3)transform.InverseTransformPoint(Transform.Position + b), visibleCells.ToHashSet(), cellChanges);
            return cellChanges;
        }

        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }
        public NitroxVector3? LastStoredPosition { get; set; }

        public Player(ushort id, string name, bool isPermaDeath, PlayerContext playerContext, NitroxConnection connection, NitroxVector3 position, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats, IEnumerable<EquippedItemData> equippedItems,
                      IEnumerable<EquippedItemData> modules)
        {
            Transform = new NitroxTransform(position, new NitroxQuaternion(0, 0, 0, 1), NitroxVector3.One, playerId);
            Id = id;
            Name = name;
            IsPermaDeath = isPermaDeath;
            PlayerContext = playerContext;
            this.connection = connection;
            SubRootId = subRootId;
            Permissions = perms;
            Stats = stats;
            LastStoredPosition = null;
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
            connection.SendPacket(packet);
        }

        public void Teleport(NitroxVector3 destination)
        {
            PlayerTeleported playerTeleported = new PlayerTeleported(Name, Transform.Position, destination);

            SendPacket(playerTeleported);
            Transform.Position = playerTeleported.DestinationTo;
            LastStoredPosition = playerTeleported.DestinationFrom;
        }

        public override string ToString()
        {
            return $"[Player - Id: {Id}, Name: {Name}, Perms: {Permissions}, Position: {Transform.Position}]";
        }

        protected bool Equals(Player other)
        {
            return Id == other.Id;
        }
    }
}
