using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel.Packets
{
    [Serializable]
    public class MapRoomScan : Packet
    {
        public NitroxId NitroxId;
        public NitroxTechType NitroxTechType;
        public bool ScanAction;

        public MapRoomScan(NitroxId nitroxId, NitroxTechType nitroxTechType, bool scanAction) : base ()
        {
            NitroxId = nitroxId;
            NitroxTechType = nitroxTechType;
            ScanAction = scanAction;
        }

        public MapRoomScan(NitroxId nitroxId, bool scanAction) : this(nitroxId, null, scanAction)
        { }
    }
}
