using NitroxModel.DataStructures.GameLogic;
using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BroadcastEscapePods : Packet
    {
        public List<EscapePodModel> EscapePods { get; private set; }

        public BroadcastEscapePods(List<EscapePodModel> escapePods) : base()
        {
            this.EscapePods = escapePods;
        }

        public override string ToString()
        {
            String toString = "[BroadcastEscapePods ";
                
            foreach(EscapePodModel model in EscapePods)
            {
                toString += model + " ";
            }

            return toString + "]";
        }
    }
}
