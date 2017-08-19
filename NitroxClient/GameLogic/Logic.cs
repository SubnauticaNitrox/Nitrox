using NitroxClient.Communication;

namespace NitroxClient.GameLogic
{
    public class Logic
    {
        public Building Building { get; private set; }
        public Chat Chat { get; private set; }
        public MedkitFabricator MedkitFabricator { get; private set; }
        public Item Item { get; private set; }
        public EquipmentSlots EquipmentSlots { get; private set; }
        public ItemContainers ItemContainers { get; private set; }
        public PlayerAttributes PlayerAttributes { get; private set; }
        public Power Power { get; private set; }
        public SimulationOwnership SimulationOwnership { get; private set; }
        public Crafting Crafting { get; private set; }
        public Cyclops Cyclops { get; private set; }
        public Interior Interior { get; private set; }

        public Logic(PacketSender packetSender)
        {
            this.Building = new Building(packetSender);
            this.Chat = new Chat(packetSender);
            this.MedkitFabricator = new MedkitFabricator(packetSender);
            this.Item = new Item(packetSender);
            this.EquipmentSlots = new EquipmentSlots(packetSender);
            this.ItemContainers = new ItemContainers(packetSender);
            this.PlayerAttributes = new PlayerAttributes(packetSender);
            this.Power = new Power(packetSender);
            this.SimulationOwnership = new SimulationOwnership(packetSender);
            this.Crafting = new Crafting(packetSender);
            this.Cyclops = new Cyclops(packetSender);
            this.Interior = new Interior(packetSender);
        }
    }
}
