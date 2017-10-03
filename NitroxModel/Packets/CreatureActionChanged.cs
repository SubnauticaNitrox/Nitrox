using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class CreatureActionChanged : Packet
    {
        public String NewAction { get; }

        public CreatureActionChanged(String newAction) : base()
        {
            this.NewAction = newAction;
        }
    }
}
