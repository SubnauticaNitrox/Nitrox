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
        public List<VehicleModel> Vehicles { get; }
        public List<ItemData> InventoryItems { get; }
        public string InventoryGuid { get; }
<<<<<<< HEAD
<<<<<<< HEAD
        public PDAStateData PDASaveData { get; }

        public InitialPlayerSync(string inventoryGuid, List<EquippedItemData> equipment, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems, DataStructures.GameLogic.PDAStateData pdaSaveData)
=======
        public PDASaveData PDASaveData { get; }

        public InitialPlayerSync(string inventoryGuid, List<EquippedItemData> equipment, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems, PDASaveData pdaSaveData)
>>>>>>> 08eed5b... Sync And Save (KnownTech Entries,PDAScanner Entries,PDAEncyclopediaEntries )
=======
        public PDAStateData PDASaveData { get; }

        public InitialPlayerSync(string inventoryGuid, List<EquippedItemData> equipment, List<BasePiece> basePieces, List<VehicleModel> vehicles, List<ItemData> inventoryItems, DataStructures.GameLogic.PDAStateData pdaSaveData)
>>>>>>> c7606c2... Changes Requested
        {
            InventoryGuid = inventoryGuid;
            EquippedItems = equipment;
            BasePieces = basePieces;
            Vehicles = vehicles;
            InventoryItems = inventoryItems;
            PDASaveData = pdaSaveData;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - EquippedItems: " + EquippedItems + " BasePieces: " + BasePieces + " Vehicles: " + Vehicles + " InventoryItems: " + InventoryItems + "]";
        }
    }
}
