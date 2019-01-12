using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Entities;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Players;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Serialization.World
{
    public class World
    {
        public PlayerManager PlayerManager { get; set; }
        public TimeKeeper TimeKeeper { get; set; }
        public SimulationOwnershipData SimulationOwnershipData { get; set; }
        public EscapePodData EscapePodData { get; set; }
        public EscapePodManager EscapePodManager { get; set; }
        public BatchEntitySpawner BatchEntitySpawner { get; set; }
        public EntityData EntityData { get; set; }
        public EntitySimulation EntitySimulation { get; set; }
        public EntityManager EntityManager { get; set; }
        public EventTriggerer EventTriggerer { get; set; }
        public BaseData BaseData { get; set; }
        public VehicleData VehicleData { get; set; }
        public InventoryData InventoryData { get; set; }
        public PlayerData PlayerData { get; set; }
        public GameData GameData { get; set; }
        public GameModeOption GameMode { get; set; }
    }
}
