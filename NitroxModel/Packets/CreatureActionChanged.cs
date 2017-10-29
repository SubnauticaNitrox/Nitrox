using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreatureActionChanged : Packet
    {
        public string NewAction { get; }

        public CreatureActionChanged(string newAction) : base()
        {
            NewAction = newAction;
        }
    }
}
