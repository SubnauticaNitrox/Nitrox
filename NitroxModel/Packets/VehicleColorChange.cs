using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class VehicleColorChange : Packet
    {
        public string Guid { get; }
        public int Index { get; }
        public Vector3 HSB { get; }
        public Color Color { get; }

        public VehicleColorChange(int index, string guid, Vector3 hsb, Color color)
        {
            Guid = guid;
            Index = index;
            HSB = hsb;
            Color = color;
        }

        public override string ToString()
        {
            return "[VehicleColorChange Guid: " + Guid + " Index: " + Index + " hsb: " + HSB + " Color: " + Color + "]";
        }
    }
}
