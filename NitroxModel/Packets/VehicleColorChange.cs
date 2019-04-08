using System;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleColorChange : Packet
    {
        public NitroxId Id { get; }
        public int Index { get; }
        public Vector3 HSB { get; }
        public Color Color { get; }

        public VehicleColorChange(int index, NitroxId id, Vector3 hsb, Color color)
        {
            Id = id;
            Index = index;
            HSB = hsb;
            Color = color;
        }

        public override string ToString()
        {
            return "[VehicleColorChange Id: " + Id + " Index: " + Index + " hsb: " + HSB + " Color: " + Color + "]";
        }
    }
}
