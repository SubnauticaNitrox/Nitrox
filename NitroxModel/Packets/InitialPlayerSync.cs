using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }

        public InitialPlayerSync(List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems)
        {
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + "]";
        }
    }
}
