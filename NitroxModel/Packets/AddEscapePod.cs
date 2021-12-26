using NitroxModel.DataStructures.GameLogic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class AddEscapePod : Packet
    {
        [Index(0)]
        public virtual EscapePodModel EscapePod { get; protected set; }

        private AddEscapePod() { }

        public AddEscapePod(EscapePodModel escapePod)
        {
            EscapePod = escapePod;
        }
    }
}
