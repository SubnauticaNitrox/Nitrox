using System.Collections.Generic;
using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class PlayerQuickSlotsBindingChanged : Packet
    {
        [Index(0)]
        public virtual List<string> Binding { get; protected set; }

        public PlayerQuickSlotsBindingChanged() { }

        public PlayerQuickSlotsBindingChanged(List<string> binding)
        {
            Binding = binding;
        }
    }
}
