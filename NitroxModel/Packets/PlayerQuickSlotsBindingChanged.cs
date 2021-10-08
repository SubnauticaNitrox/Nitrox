using System;
using System.Collections.Generic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerQuickSlotsBindingChanged : Packet
    {
        public List<string> Binding { get; }

        public PlayerQuickSlotsBindingChanged(List<string> binding)
        {
            Binding = binding;
        }
    }
}
