using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
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
        public IMultiplayerSessionManager MultiplayerSessionManager { get; }

        public Logic(IMultiplayerSessionManager multiplayerSessionManager, VisibleCells visibleCells, DeferringPacketReceiver packetReceiver)
        {
            Log.Info("Initializing Multiplayer GameLogic...");
            AI = new AI(multiplayerSessionManager);
            Building = new Building(multiplayerSessionManager);
            Chat = new Chat(multiplayerSessionManager);
            Entities = new Entities(multiplayerSessionManager);
            MedkitFabricator = new MedkitFabricator(multiplayerSessionManager);
            Item = new Item(multiplayerSessionManager);
            EquipmentSlots = new EquipmentSlots(multiplayerSessionManager);
            ItemContainers = new ItemContainers(multiplayerSessionManager);
            Player = new PlayerLogic(multiplayerSessionManager);
            Power = new Power(multiplayerSessionManager);
            SimulationOwnership = new SimulationOwnership(multiplayerSessionManager);
            Crafting = new Crafting(multiplayerSessionManager);
            Cyclops = new Cyclops(multiplayerSessionManager);
            Interior = new Interior(multiplayerSessionManager);
            MobileVehicleBay = new MobileVehicleBay(multiplayerSessionManager);
            Terrain = new Terrain(multiplayerSessionManager, visibleCells, packetReceiver);
            PacketSender = multiplayerSessionManager;
            MultiplayerSessionManager = multiplayerSessionManager;
            Log.Info("Multiplayer GameLogic Initialized...");
        }
    }
}
