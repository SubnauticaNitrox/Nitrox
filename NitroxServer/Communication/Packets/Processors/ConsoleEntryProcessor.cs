using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;
using NitroxServer.GameLogic.Items;

namespace NitroxServer.Communication.Packets.Processors
{
    public class ConsoleEntryProcessor : AuthenticatedPacketProcessor<ConsoleEntry>
    {
        public ConsoleEntryProcessor(PlayerManager playerManager)
        {
        }

        public override void Process(ConsoleEntry packet,Player player)
        {
            Program._Server.Save();
        }
    }
}
