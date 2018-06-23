using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;

namespace NitroxServer.Serialization.World
{
    public class World
    {
        public PlayerManager PlayerManager { get; set; }
        public TimeKeeper TimeKeeper { get; set; }
        public SimulationOwnership SimulationOwnership { get; set; }
        public EscapePodManager EscapePodManager { get; set; }
        public BatchEntitySpawner BatchEntitySpawner { get; set; }
        public EntityData EntityData { get; set; }
        public EntitySimulation EntitySimulation { get; set; }
        public EntityManager EntityManager { get; set; }
        public EventTriggerer EventTriggerer { get; set; }
        public BaseData BaseData { get; set; }
    }
}
