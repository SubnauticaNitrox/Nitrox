using System;
using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Server;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public NitroxId AssignedEscapePodId { get; }
        public Dictionary<string, NitroxId> EquippedItems { get; }
        public List<NitroxTechType> UsedItems { get; }
        public Optional<NitroxId>[] QuickSlotsBindingIds { get; }
        public NitroxId PlayerGameObjectId { get; }
        public bool FirstTimeConnecting { get; }
        public InitialPDAData PDAData { get; }
        public InitialStoryGoalData StoryGoalData { get; }
        public NitroxVector3 PlayerSpawnData { get; }
        public NitroxQuaternion PlayerSpawnRotation { get; }
        public Optional<NitroxId> PlayerSubRootId { get; }
        public PlayerStatsData PlayerStatsData { get; }
        public List<PlayerContext> OtherPlayers { get; }
        public List<Entity> GlobalRootEntities { get; }
        public List<SimulatedEntity> InitialSimulationOwnerships { get; }
        public NitroxGameMode GameMode { get; }
        public Perms Permissions { get; }
        public IntroCinematicMode IntroCinematicMode { get; }
        public SubnauticaPlayerPreferences Preferences { get; }
        public TimeData TimeData { get; }
        public bool IsFirstPlayer { get; }
        public Dictionary<NitroxId, int> BuildOperationIds { get; }

        public bool KeepInventoryOnDeath { get; }
        public SessionSettings SessionSettings { get; }

        public InitialPlayerSync(NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            NitroxId assignedEscapePodId,
            IDictionary<string, NitroxId> equipment,
            IEnumerable<NitroxTechType> usedItems,
            Optional<NitroxId>[] quickSlotsBindingIds,
            InitialPDAData pdaData,
            InitialStoryGoalData storyGoalData,
            NitroxVector3 playerSpawnData,
            NitroxQuaternion playerSpawnRotation,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            IEnumerable<PlayerContext> otherPlayers,
            IEnumerable<Entity> globalRootEntities,
            IEnumerable<SimulatedEntity> initialSimulationOwnerships,
            NitroxGameMode gameMode,
            Perms perms,
            IntroCinematicMode introCinematicMode,
            SubnauticaPlayerPreferences preferences,
            TimeData timeData,
            bool isFirstPlayer,
            Dictionary<NitroxId, int> buildOperationIds,
            bool keepInventoryOnDeath,
            SessionSettings sessionSettings)
        {
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = new(equipment);
            UsedItems = usedItems.ToList();
            QuickSlotsBindingIds = quickSlotsBindingIds;
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            PlayerSpawnData = playerSpawnData;
            PlayerSpawnRotation = playerSpawnRotation;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            OtherPlayers = otherPlayers.ToList();
            GlobalRootEntities = globalRootEntities.ToList();
            InitialSimulationOwnerships = initialSimulationOwnerships.ToList();
            GameMode = gameMode;
            Permissions = perms;
            IntroCinematicMode = introCinematicMode;
            Preferences = preferences;
            TimeData = timeData;
            IsFirstPlayer = isFirstPlayer;
            BuildOperationIds = buildOperationIds;
            KeepInventoryOnDeath = keepInventoryOnDeath;
            SessionSettings = sessionSettings;
        }

        /// <remarks>Used for deserialization</remarks>
        public InitialPlayerSync(
            NitroxId assignedEscapePodId,
            Dictionary<string, NitroxId> equippedItems,
            List<NitroxTechType> usedItems,
            Optional<NitroxId>[] quickSlotsBindingIds,
            NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            InitialPDAData pdaData,
            InitialStoryGoalData storyGoalData,
            NitroxVector3 playerSpawnData,
            NitroxQuaternion playerSpawnRotation,
            Optional<NitroxId> playerSubRootId,
            PlayerStatsData playerStatsData,
            List<PlayerContext> otherPlayers,
            List<Entity> globalRootEntities,
            List<SimulatedEntity> initialSimulationOwnerships,
            NitroxGameMode gameMode,
            Perms permissions,
            IntroCinematicMode introCinematicMode,
            SubnauticaPlayerPreferences preferences,
            TimeData timeData,
            bool isFirstPlayer,
            Dictionary<NitroxId, int> buildOperationIds,
            bool keepInventoryOnDeath,
            SessionSettings sessionSettings)
        {
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equippedItems;
            UsedItems = usedItems;
            QuickSlotsBindingIds = quickSlotsBindingIds;
            PDAData = pdaData;
            StoryGoalData = storyGoalData;
            PlayerSpawnData = playerSpawnData;
            PlayerSpawnRotation = playerSpawnRotation;
            PlayerSubRootId = playerSubRootId;
            PlayerStatsData = playerStatsData;
            OtherPlayers = otherPlayers;
            GlobalRootEntities = globalRootEntities;
            InitialSimulationOwnerships = initialSimulationOwnerships;
            GameMode = gameMode;
            Permissions = permissions;
            IntroCinematicMode = introCinematicMode;
            Preferences = preferences;
            TimeData = timeData;
            IsFirstPlayer = isFirstPlayer;
            BuildOperationIds = buildOperationIds;
            KeepInventoryOnDeath = keepInventoryOnDeath;
            SessionSettings = sessionSettings;
        }
    }
}
