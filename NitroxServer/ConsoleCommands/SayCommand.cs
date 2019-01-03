using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;
using NitroxModel.Packets;
using NitroxModel.Logger;

namespace NitroxServer.ConsoleCommands
{
    class SayCommand : Command
    {
        private PlayerManager playerManager;

        public SayCommand(PlayerManager playerManager) : base("say", "say Even the lowliest of cogs needs to say something SO SAY SOMETHING!", new string[] { "broadcast" })
        {
            this.playerManager = playerManager;
        }

        public override void RunCommand(string[] args)
        {
            playerManager.SendPacketToAllPlayers(new ChatMessage(ushort.MaxValue, string.Join(" ", args)));
            Log.Info("Server says: " + string.Join(" ", args));
        }

        public override bool VerifyArgs(string[] args)
        {
            return (args.Length > 0);
        }
    }
}
