using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeColor : Packet
    {
        public string Guid { get; }
        public int Index { get; }
        public Vector3 HSB { get; }
        public Color Color { get; }

        public CyclopsChangeColor(string playerId, int index, string guid, Vector3 hsb, Color color)
        {
            Guid = guid;
            Index = index;
            HSB = hsb;
            Color = color;
        }

        public override string ToString()
        {
            return "[CyclopsChangeColor Guid: " + Guid + " Index: " + Index + " hsb: " + HSB + " Color: " + Color + "]";
        }
    }
}
