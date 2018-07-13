using System;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleRemoveEntry : Packet
    {
        public string Guid { get; }

        public VehicleRemoveEntry(string guid)
        {
            Guid = guid;
        }

        public override string ToString()
        {
            return "[VehicleRemove - Vehicle Guid: " + Guid + "]";
        }
    }
}
