using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.Logger;

namespace NitroxClient.GameLogic
{
    public class Logic
    {
        public AI AI { get; }
        public Building Building { get; }
        public Chat Chat { get; }
        public Entities Entities { get; }
        public MedkitFabricator MedkitFabricator { get; }
        public Item Item { get; }
        public EquipmentSlots EquipmentSlots { get; }
        public ItemContainers ItemContainers { get; }
        public PlayerLogic Player { get; }
        public Power Power { get; }
        public SimulationOwnership SimulationOwnership { get; }
        public Crafting Crafting { get; }
        public Cyclops Cyclops { get; }
        public Interior Interior { get; }
        public MobileVehicleBay MobileVehicleBay { get; }
        public Terrain Terrain { get; }
        public IPacketSender PacketSender { get; }
        public IClientBridge ClientBridge { get; }
        public ServerTime ServerTime { get; }

        public Logic(IClientBridge clientBridge, VisibleCells visibleCells, DeferringPacketReceiver packetReceiver)
        {
            Log.Info("Initializing Multiplayer GameLogic...");
            AI = new AI(clientBridge);
            Building = new Building(clientBridge);
            Chat = new Chat(clientBridge);
            Entities = new Entities(clientBridge);
            MedkitFabricator = new MedkitFabricator(clientBridge);
            Item = new Item(clientBridge);
            EquipmentSlots = new EquipmentSlots(clientBridge);
            ItemContainers = new ItemContainers(clientBridge);
            Player = new PlayerLogic(clientBridge);
            Power = new Power(clientBridge);
            SimulationOwnership = new SimulationOwnership(clientBridge);
            Crafting = new Crafting(clientBridge);
            Cyclops = new Cyclops(clientBridge);
            Interior = new Interior(clientBridge);
            MobileVehicleBay = new MobileVehicleBay(clientBridge);
            Terrain = new Terrain(clientBridge, visibleCells, packetReceiver);
            PacketSender = clientBridge;
            ClientBridge = clientBridge;
            ServerTime = new ServerTime(clientBridge);
            Log.Info("Multiplayer GameLogic Initialized...");
        }
    }
}
