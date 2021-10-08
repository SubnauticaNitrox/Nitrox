using System;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class AddEscapePod : Packet
    {
        public EscapePodModel EscapePod { get; }

        public AddEscapePod(EscapePodModel escapePod)
        {
            EscapePod = escapePod;
        }
    }
}
