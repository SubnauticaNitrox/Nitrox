using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxServer.Communication;
using NitroxServer.GameLogic.Players;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly ThreadSafeList<EquippedItemData> equippedItems;
        private readonly ThreadSafeList<EquippedItemData> modules;
        private readonly ThreadSafeSet<AbsoluteEntityCell> visibleCells;

        public ThreadSafeList<NitroxTechType> UsedItems { get; }
        public ThreadSafeList<string> QuickSlotsBinding { get; set; }

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
        public ThreadSafeSet<string> CompletedGoals { get; }
        public PingInstancePreferences PingInstancePreferences { get; set; }

        public Player(ushort id, string name, bool isPermaDeath, PlayerContext playerContext, NitroxConnection connection,
                      NitroxVector3 position, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats,
                      IEnumerable<NitroxTechType> usedItems, IEnumerable<string> quickSlotsBinding,
                      IEnumerable<EquippedItemData> equippedItems, IEnumerable<EquippedItemData> modules, HashSet<string> completedGoals, PingInstancePreferences pingInstancePreferences)
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
            UsedItems = new ThreadSafeList<NitroxTechType>(usedItems);
            QuickSlotsBinding = new ThreadSafeList<string>(quickSlotsBinding);
            this.equippedItems = new ThreadSafeList<EquippedItemData>(equippedItems);
            this.modules = new ThreadSafeList<EquippedItemData>(modules);
            visibleCells = new ThreadSafeSet<AbsoluteEntityCell>();
            CompletedGoals = new ThreadSafeSet<string>(completedGoals);
            PingInstancePreferences  = pingInstancePreferences;
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

        public void SetPingVisible(string pingKey, bool visibility)
        {
            // Default behaviour which we don't want to notice
            if (visibility)
            {
                PingInstancePreferences.HiddenSignalPings.Remove(pingKey);
                return;
            }

            PingInstancePreferences.HiddenSignalPings.Add(pingKey);
        }

        public void SetPingColor(string pingKey, int color)
        {
            // Default color which we don't want to notice
            if (color == 0)
            {
                PingInstancePreferences.ColorPreferences.Remove(pingKey);
                return;
            }

            PingInstancePreferences.ColorPreferences[pingKey] = color;
        }
    }
}
