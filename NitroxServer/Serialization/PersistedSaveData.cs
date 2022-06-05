using Newtonsoft.Json;
using NitroxServer.GameLogic;
using NitroxServer.Serialization.SaveData;

namespace NitroxServer.Serialization;

[JsonObject(MemberSerialization.OptIn)]
public class PersistedSaveData
{
    [JsonProperty]
    public WorldData WorldData { get; set; }

    [JsonProperty]
    public BaseData BaseData { get; set; }

    [JsonProperty]
    public PlayerData PlayerData { get; set; }

    [JsonProperty]
    public EntityData EntityData { get; set; }

    public static PersistedSaveData From(World world)
    {
        return new PersistedSaveData
        {
            BaseData = BaseData.From(world.BaseManager.GetPartiallyConstructedPieces(), world.BaseManager.GetCompletedBasePieceHistory()),
            PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers()),
            EntityData = EntityData.From(world.EntityManager.GetAllEntities()),
            WorldData = new WorldData
            {
                ParsedBatchCells = world.BatchEntitySpawner.SerializableParsedBatches,
                VehicleData = VehicleData.From(world.VehicleManager.GetVehicles()),
                InventoryData = InventoryData.From(world.InventoryManager.GetAllInventoryItems(), world.InventoryManager.GetAllStorageSlotItems(), world.InventoryManager.GetAllModules()),
                GameData = GameData.From(world.GameData.PDAState, world.GameData.StoryGoals, world.ScheduleKeeper, world.EventTriggerer),
                EscapePodData = EscapePodData.From(world.EscapePodManager.GetEscapePods()),
                Seed = world.Seed
            }
        };
    }

    public bool IsValid()
    {
        return WorldData != null &&
               WorldData.IsValid() &&
               BaseData != null &&
               PlayerData != null &&
               EntityData != null;
    }
}
