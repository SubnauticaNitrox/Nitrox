using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Items;
using NitroxServer.GameLogic.Vehicles;

namespace NitroxServer.Serialization.World
{
    [DataContract]
    public class WorldData
    {
        [DataMember(Order = 1)]
        public List<NitroxInt3> ParsedBatchCells { get; set; }

        [DataMember(Order = 2)]
        public VehicleData VehicleData { get; set; }

        [DataMember(Order = 3)]
        public InventoryData InventoryData { get; set; }

        [DataMember(Order = 4)]
        public GameData GameData { get; set; }

        [DataMember(Order = 5)]
        public string Seed { get; set; }

        public bool IsValid()
        {
            return ParsedBatchCells != null && // Always returns false on empty saves (sometimes also if never entered the ocean)
                   VehicleData != null &&
                   InventoryData != null &&
                   GameData != null;
        }
    }
}
