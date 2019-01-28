using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class FireDouse : Packet
    {
        public Vector3 FirePosition { get; }
        public float DouseAmount { get; }
        public bool Extinguished { get; }

        public FireDouse(Vector3 firePosition, float douseAmount, bool extinguished)
        {
            FirePosition = firePosition;
            DouseAmount = douseAmount;
            Extinguished = extinguished;
        }

        public override string ToString()
        {
            return "[FireDouse - FirePosition: " + FirePosition + " DouseAmount: " + DouseAmount + " Extinguished: " + Extinguished + "]";
        }
    }
}
