using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic;
using NitroxClient.Logger;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors
{
    class ConnectProcessor : GenericPacketProcessor<Connect>
    {
        public ConnectProcessor()
        {
        }

        public override void Process(Connect connect)
        {
            // Future: Init new player here
            ClientLogger.WriteLine(connect.PlayerId + " connected");
        }
    }
}
