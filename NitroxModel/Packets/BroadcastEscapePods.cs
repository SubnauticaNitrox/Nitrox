using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class BroadcastEscapePods : Packet
    {
        public EscapePodModel[] EscapePods { get; }

        public BroadcastEscapePods(EscapePodModel[] escapePods) : base()
        {
            EscapePods = escapePods;
        }

        public override string ToString()
        {
            string toString = "[BroadcastEscapePods ";

            foreach (EscapePodModel model in EscapePods)
            {
                toString += model + " ";
            }

            return toString + "]";
        }
    }
}
