using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Bases;

namespace NitroxServer.ConsoleCommands
{
    internal class SyncIds : Command
    {
        public SyncIds() : base("sync", Perms.ADMIN, "", "Sync client data")
        {
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            
            Log.Info("Syncing Client Ids to ther server database...");
        }


        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }
    }
}
