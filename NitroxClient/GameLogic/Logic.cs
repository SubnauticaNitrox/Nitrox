using NitroxClient.Communication;
using NitroxClient.Map;

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

        public Logic(IPacketSender packetSender, VisibleCells visibleCells, DeferringPacketReceiver packetReceiver)
        {
            AI = new AI(packetSender);
            Building = new Building(packetSender);
            Chat = new Chat(packetSender);
            Entities = new Entities(packetSender);
            MedkitFabricator = new MedkitFabricator(packetSender);
            Item = new Item(packetSender);
            EquipmentSlots = new EquipmentSlots(packetSender);
            ItemContainers = new ItemContainers(packetSender);
            Player = new PlayerLogic(packetSender);
            Power = new Power(packetSender);
            SimulationOwnership = new SimulationOwnership(packetSender);
            Crafting = new Crafting(packetSender);
            Cyclops = new Cyclops(packetSender);
            Interior = new Interior(packetSender);
            MobileVehicleBay = new MobileVehicleBay(packetSender);
            Terrain = new Terrain(packetSender, visibleCells, packetReceiver);
            PacketSender = packetSender;
        }
    }
}
