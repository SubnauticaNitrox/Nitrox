using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreaturePositionsChanged : Packet
    {
        public Dictionary<String, Transform> GuidsWithTransform = new Dictionary<string, Transform>();
        
        public CreaturePositionsChanged(Dictionary<String, Transform> guidsWithTransform) : base()
        {
            GuidsWithTransform = guidsWithTransform;
        }

        public override string ToString()
        {
            String toString = "[CreaturePositionsChanged -";

            foreach(var guidWithTransform in GuidsWithTransform)
            {
                toString += "{" + guidWithTransform.Key + ": " + guidWithTransform.Value + "}, ";
            }

            return toString + "]";
        }
    }
}
