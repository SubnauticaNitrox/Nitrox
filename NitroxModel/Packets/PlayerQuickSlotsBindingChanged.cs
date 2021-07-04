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

        public override string ToString()
        {
            return $"[PlayerQuickSlotsBindingChanged - Binding: {Binding.Count}]";
        }
    }
}
