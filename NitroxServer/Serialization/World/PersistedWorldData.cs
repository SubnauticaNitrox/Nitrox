using System.Runtime.Serialization;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Serialization.World
{
    [DataContract]
    public class PersistedWorldData
    {
        [DataMember(Order = 1)]
        public WorldData WorldData { get; set; } = new WorldData();

        [DataMember(Order = 2)]
        public BaseData BaseData { get; set; }

        [DataMember(Order = 3)]
        public PlayerData PlayerData { get; set; }

        [DataMember(Order = 4)]
        public EntityData EntityData { get; set; }

        public static PersistedWorldData From(World world)
        {
            return new PersistedWorldData
            {
                BaseData = BaseData.From(world.BaseManager.GetPartiallyConstructedPieces(), world.BaseManager.GetCompletedBasePieceHistory()),
                PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers()),
                EntityData = EntityData.From(world.EntityRegistry.GetAllEntities()),
                WorldData =
                {
                    ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches,
                    VehicleData = VehicleData.From(world.VehicleManager.GetVehicles()),
                    InventoryData = InventoryData.From(world.InventoryManager.GetAllStorageSlotItems(), world.InventoryManager.GetAllModules()),
                    GameData = GameData.From(world.GameData.PDAState, world.GameData.StoryGoals, world.ScheduleKeeper, world.EventTriggerer),
                    Seed = world.Seed
                }
            };
        }

        public bool IsValid()
        {
            return WorldData.IsValid() &&
                   BaseData != null &&
                   PlayerData != null &&
                   EntityData != null;
        }
    }
}
