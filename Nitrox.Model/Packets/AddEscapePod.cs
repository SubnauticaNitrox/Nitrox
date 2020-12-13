using System;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Model.Packets
{
    [Serializable]
    public class AddEscapePod : Packet
    {
        public EscapePodModel EscapePod { get; }

        public AddEscapePod(EscapePodModel escapePod)
        {
            EscapePod = escapePod;
        }

        public override string ToString()
        {
            return "[AddEscapePod " + EscapePod + " ]";
        }
    }
}
