using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerQuickSlotsBindingChanged : Packet
    {
        public string[] Binding { get; }

        public PlayerQuickSlotsBindingChanged(string[] binding)
        {
            Binding = binding;
        }

        public override string ToString()
        {
            return $"[PlayerQuickSlotsBindingChanged - Binding: {Binding.Length}]";
        }
    }
}
