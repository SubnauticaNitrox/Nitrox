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
        public List<EquippedItemData> EquippedItems { get; }
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
        public List<NitroxId> InitialSimulationOwnerships { get; }
        public NitroxGameMode GameMode { get; }
        public Perms Permissions { get; }
        public SubnauticaPlayerPreferences Preferences { get; }
        public TimeData TimeData { get; }
        public Dictionary<NitroxId, int> BuildOperationIds { get; }

        public InitialPlayerSync(NitroxId playerGameObjectId,
            bool firstTimeConnecting,
            NitroxId assignedEscapePodId,
            IEnumerable<EquippedItemData> equipment,
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
            IEnumerable<NitroxId> initialSimulationOwnerships,
            NitroxGameMode gameMode,
            Perms perms,
            SubnauticaPlayerPreferences preferences,
            TimeData timeData,
            Dictionary<NitroxId, int> buildOperationIds)
        {
            AssignedEscapePodId = assignedEscapePodId;
            PlayerGameObjectId = playerGameObjectId;
            FirstTimeConnecting = firstTimeConnecting;
            EquippedItems = equipment.ToList();
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
            Preferences = preferences;
            TimeData = timeData;
            BuildOperationIds = buildOperationIds;
        }

        /// <remarks>Used for deserialization</remarks>
        public InitialPlayerSync(
            NitroxId assignedEscapePodId,
            List<EquippedItemData> equippedItems,
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
            List<NitroxId> initialSimulationOwnerships,
            NitroxGameMode gameMode,
            Perms permissions,
            SubnauticaPlayerPreferences preferences,
            TimeData timeData,
            Dictionary<NitroxId, int> buildOperationIds)
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
            Preferences = preferences;
            TimeData = timeData;
            BuildOperationIds = buildOperationIds;
        }
    }
}
