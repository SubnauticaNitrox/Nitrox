using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic.Entities.Spawning;
using NitroxServer.Serialization;

namespace NitroxServer.ConsoleCommands
{
    class ParseBatchCommand : Command
    {
        public ParseBatchCommand() : base("parsebatch", NitroxModel.DataStructures.GameLogic.Perms.CONSOLE)
        {}

        public override void RunCommand(string[] args, Optional<Player> player)
        {
            BatchEntitySpawner parser = NitroxModel.Core.NitroxServiceLocator.LocateService<BatchEntitySpawner>();
            parser.LoadUnspawnedEntities(new Int3(int.Parse(args[0]), int.Parse(args[1]), int.Parse(args[2])));
        }

        public override bool VerifyArgs(string[] args)
        {
            return true;
        }
    }
}
