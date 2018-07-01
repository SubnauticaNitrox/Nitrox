using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class InitialPlayerSync : Packet
    {
        public List<EquippedItemData> EquippedItems { get; }
        public List<BasePiece> BasePieces { get; }
        public List<SignData> SignData { get; }
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public string InventoryGuid { get; }

        public InitialPlayerSync(string inventoryGuid, List<EquippedItemData> equipment, List<BasePiece> basePieces, List<SignData> singdata, List<VehicleModel> vehicles, List<ItemData> inventoryItems)
        {
            InventoryGuid = inventoryGuid;
            EquippedItems = equipment;
            BasePieces = basePieces;
            SignData = singdata;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - EquippedItems: " + EquippedItems + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + "]";
        }
    }
}
