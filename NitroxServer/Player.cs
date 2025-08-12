using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NitroxModel.Packets.Processors.Abstract;
using NitroxModel.Server;
using NitroxServer.Communication;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        private readonly ThreadSafeSet<AbsoluteEntityCell> visibleCells;

        public ThreadSafeList<NitroxTechType> UsedItems { get; }
        public Optional<NitroxId>[] QuickSlotsBindingIds { get; set; }

        public INitroxConnection Connection { get; set; }
        public PlayerSettings PlayerSettings => PlayerContext.PlayerSettings;
        public PlayerContext PlayerContext { get; set; }
        public ushort Id { get; }
        public string Name { get; }
        public bool IsPermaDeath { get; set; }
        public NitroxVector3 Position { get; set; }
        public NitroxQuaternion Rotation { get; set; }
        public NitroxId GameObjectId { get; set; }
        public Optional<NitroxId> SubRootId { get; set; }
        public Perms Permissions { get; set; }
        public PlayerStatsData Stats { get; set; }
        public NitroxGameMode GameMode { get; set; }
        public NitroxVector3? LastStoredPosition { get; set; }
        public Optional<NitroxId> LastStoredSubRootID { get; set; }
        public ThreadSafeDictionary<string, float> PersonalCompletedGoalsWithTimestamp { get; }
        public ThreadSafeDictionary<string, PingInstancePreference> PingInstancePreferences { get; set; }
        public ThreadSafeList<int> PinnedRecipePreferences { get; set; }
        public ThreadSafeDictionary<string, NitroxId> EquippedItems { get; set ;}
        public ThreadSafeSet<NitroxId> OutOfCellVisibleEntities { get; set; } = [];

        public PlayerWorldEntity Entity { get; set; }

        public Player(ushort id, string name, bool isPermaDeath, PlayerContext playerContext, INitroxConnection connection,
                      NitroxVector3 position, NitroxQuaternion rotation, NitroxId playerId, Optional<NitroxId> subRootId, Perms perms, PlayerStatsData stats, NitroxGameMode gameMode,
                      IEnumerable<NitroxTechType> usedItems, Optional<NitroxId>[] quickSlotsBindingIds,
                      IDictionary<string, NitroxId> equippedItems, IDictionary<string, float> personalCompletedGoalsWithTimestamp, IDictionary<string, PingInstancePreference> pingInstancePreferences, IList<int> pinnedRecipePreferences)
        {
            Id = id;
            Name = name;
            IsPermaDeath = isPermaDeath;
            PlayerContext = playerContext;
            Connection = connection;
            Position = position;
            Rotation = rotation;
            SubRootId = subRootId;
            GameObjectId = playerId;
            Permissions = perms;
            Stats = stats;
            GameMode = gameMode;
            LastStoredPosition = null;
            LastStoredSubRootID = Optional.Empty;
            UsedItems = new ThreadSafeList<NitroxTechType>(usedItems);
            QuickSlotsBindingIds = quickSlotsBindingIds;
            EquippedItems = new ThreadSafeDictionary<string, NitroxId>(equippedItems);
            visibleCells = new ThreadSafeSet<AbsoluteEntityCell>();
            PersonalCompletedGoalsWithTimestamp = new ThreadSafeDictionary<string, float>(personalCompletedGoalsWithTimestamp);
            PingInstancePreferences = new(pingInstancePreferences);
            PinnedRecipePreferences = new(pinnedRecipePreferences);
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

        /// <summary>
        /// Returns a <b>new</b> list from the original set. To use the original set, use <see cref="AddCells"/>, <see cref="RemoveCells"/> and <see cref="HasCellLoaded"/>.
        /// </summary>
        internal List<AbsoluteEntityCell> GetVisibleCells()
        {
            return [.. visibleCells];
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

        public void ClearVisibleCells()
        {
            visibleCells.Clear();
        }

        public bool CanSee(Entity entity)
        {
            if (entity is WorldEntity worldEntity)
            {
                return worldEntity is GlobalRootEntity || HasCellLoaded(worldEntity.AbsoluteEntityCell) ||
                       OutOfCellVisibleEntities.Contains(entity.Id);
            }

            return true;
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
