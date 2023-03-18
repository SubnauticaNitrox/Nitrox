using NitroxModel.Server;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Serialization.World
{
    public class World
    {
        public PlayerManager PlayerManager { get; set; }
        public ScheduleKeeper ScheduleKeeper { get; set; }
        public TimeKeeper TimeKeeper { get; set; }
        public SimulationOwnershipData SimulationOwnershipData { get; set; }
        public EscapePodManager EscapePodManager { get; set; }
        public BatchEntitySpawner BatchEntitySpawner { get; set; }
        public EntitySimulation EntitySimulation { get; set; }
        public EntityRegistry EntityRegistry { get; set; }
        public WorldEntityManager WorldEntityManager { get; set; }
        public BaseManager BaseManager { get; set; }
        public StoryManager StoryManager { get; set; }
        public InventoryManager InventoryManager { get; set; }
        public GameData GameData { get; set; }
        public ServerGameMode GameMode { get; set; }
        public string Seed { get; set; }
    }
}
