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

        public InitialPlayerSync(List<BasePiece> basePieces, List<VehicleModel> vehicles)
        {
            BasePieces = basePieces;
            Vehicles = vehicles;
        }

        public override string ToString()
        {
            return "[InitialPlayerSync - BasePieces: " + BasePieces + "]";
        }
    }
}
