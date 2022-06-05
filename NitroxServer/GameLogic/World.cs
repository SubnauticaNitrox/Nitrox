using NitroxModel.Server;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.WorldTiming;
using NitroxServer.Serialization.SaveData;

namespace NitroxServer.GameLogic
{
    public class World
    {
        public PlayerManager PlayerManager { get; set; }
        public ScheduleKeeper ScheduleKeeper { get; set; }
        public SimulationOwnershipData SimulationOwnershipData { get; set; }
        public EscapePodManager EscapePodManager { get; set; }
        public BatchEntitySpawner BatchEntitySpawner { get; set; }
        public EntitySimulation EntitySimulation { get; set; }
        public EntityManager EntityManager { get; set; }
        public BaseManager BaseManager { get; set; }
        public EventTriggerer EventTriggerer { get; set; }
        public VehicleManager VehicleManager { get; set; }
        public InventoryManager InventoryManager { get; set; }
        public GameData GameData { get; set; }
        public ServerGameMode GameMode { get; set; }
        public string Seed { get; set; }
    }
}
