using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class DockingStateChange : Packet
    {
        public NitroxId NitroxId;
        public bool Docked;

        public DockingStateChange(NitroxId nitroxId, bool docked)
        {
            NitroxId = nitroxId;
            Docked = docked;
        }
    }
}
