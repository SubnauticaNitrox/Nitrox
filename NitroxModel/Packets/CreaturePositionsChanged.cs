using NitroxModel.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreaturePositionsChanged : Packet
    {
        public List<String> Guids { get { return guidsWithSerializablePosition.Keys.ToList(); } }
        
        private Dictionary<String, SerializableTransform> guidsWithSerializablePosition;

        public CreaturePositionsChanged(Dictionary<String, Transform> guidsWithTransform) : base()
        {
            guidsWithSerializablePosition = new Dictionary<String, SerializableTransform>();

            foreach(var guidWithTransform in guidsWithTransform)
            {
                guidsWithSerializablePosition.Add(guidWithTransform.Key, SerializableTransform.From(guidWithTransform.Value));
            }
        }

        public void SetTransform(Transform transform, String guid)
        {
            SerializableTransform st = guidsWithSerializablePosition[guid];
            st.SetTransform(transform);
        }

        public override string ToString()
        {
            String toString = "[CreaturePositionsChanged -";

            foreach(var guidWithTransform in guidsWithSerializablePosition)
            {
                toString += "{" + guidWithTransform.Key + ": " + guidWithTransform.Value + "}, ";
            }

            return toString + "]";
        }
    }
}
