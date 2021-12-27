using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class PlantableItemData : ItemData
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual double PlantedGameTime { get; protected set; }


        protected PlantableItemData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        /// <summary>
        /// Extends the basic ItemData by adding the game time when the Plantable was added to its Planter container.
        /// </summary>
        /// <param name="plantedGameTime">Clients will use this to determine expected plant growth progress when connecting </param>
        public PlantableItemData(NitroxId containerId, NitroxId itemId, byte[] serializedData, double plantedGameTime) : base(containerId, itemId, serializedData)
        {
            PlantedGameTime = plantedGameTime;
        }

        public override string ToString()
        {
            return $"[PlantedItemData ContainerId: {ContainerId} Id: {ItemId} Planted: {PlantedGameTime}";
        }
    }
}
