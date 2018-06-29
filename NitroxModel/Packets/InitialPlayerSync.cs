using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public string PlayerInventoryGuid { get; }
        public List<BasePiece> BasePieces { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }

        public InitialPlayerSync(string playerInventoryGuid, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems)
        {
            PlayerInventoryGuid = playerInventoryGuid;
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - PlayerInventoryGuid: " + PlayerInventoryGuid + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + "]";
        }
    }
}
