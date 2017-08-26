using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets.WorldSending
{
    public class SavedBatchCell : AuthenticatedPacket
    {
        public SavedBatchCell(string playerId) : base(playerId) {}
    }
}
