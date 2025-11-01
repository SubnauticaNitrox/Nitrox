using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Bases;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities.Spawning;

namespace Nitrox.Server.Subnautica.Models.Serialization.World
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
        public BuildingManager BuildingManager { get; set; }
        public StoryManager StoryManager { get; set; }
        public GameData GameData { get; set; }
        public SessionSettings SessionSettings { get; set; }
        public NitroxGameMode GameMode { get; set; }
        public string Seed { get; set; }
    }
}
