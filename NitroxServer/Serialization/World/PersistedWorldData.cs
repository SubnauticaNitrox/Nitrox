using Newtonsoft.Json;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Vehicles;
using ProtoBufNet;

namespace NitroxServer.Serialization.World
{
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class PersistedWorldData
    {
        [JsonProperty, ProtoMember(1)]
        public WorldData WorldData { get; set; } = new WorldData();

        [JsonProperty, ProtoMember(2)]
        public BaseData BaseData { get; set; }

        [JsonProperty, ProtoMember(3)]
        public PlayerData PlayerData { get; set; }

        [JsonProperty, ProtoMember(4)]
        public EntityData EntityData { get; set; }

        public static PersistedWorldData From(World world)
        {
            return new PersistedWorldData
            {
                BaseData = BaseData.From(world.BaseManager.GetPartiallyConstructedPieces(), world.BaseManager.GetCompletedBasePieceHistory()),
                PlayerData = PlayerData.From(world.PlayerManager.GetAllPlayers()),
                EntityData = EntityData.From(world.EntityManager.GetAllEntities()),
                WorldData =
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
            return WorldData.IsValid() &&
                   BaseData != null &&
                   PlayerData != null &&
                   EntityData != null;
        }
    }
}
