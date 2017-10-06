using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CyclopsChangeColor : AuthenticatedPacket
    {
        public String Guid { get; private set; }
        public int Index { get; private set; }
        public Vector3 HSB { get; private set; }
        public Color Color { get; private set; }

        public CyclopsChangeColor(String playerId, int index, String guid, Vector3 hsb, Color color) : base(playerId)
        {
            this.Guid = guid;
            this.Index = index;
            this.HSB = hsb;
            this.Color = color;
        }

        public override string ToString()
        {
            return "[CyclopsChangeColor PlayerId: " + PlayerId + " Guid: " + Guid + " Index: " + Index + " hsb: " + HSB + " Color: " + Color + "]";
        }
    }
}
