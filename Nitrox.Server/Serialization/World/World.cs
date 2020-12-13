using Nitrox.Model.Server;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Bases;
using Nitrox.Server.GameLogic.Entities;
using Nitrox.Server.GameLogic.Entities.Spawning;
using Nitrox.Server.GameLogic.Items;
using Nitrox.Server.GameLogic.Vehicles;

namespace Nitrox.Server.Serialization.World
{
    public class World
    {
        public PlayerManager PlayerManager { get; set; }
        public TimeKeeper TimeKeeper { get; set; }
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
    }
}
