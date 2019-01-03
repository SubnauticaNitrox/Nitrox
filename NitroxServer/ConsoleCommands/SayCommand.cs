using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.ConsoleCommands
{
    internal class SayCommand : Command
    {
        private readonly PlayerManager playerManager;

        public SayCommand(PlayerManager playerManager) : base("say", Optional<string>.Of("<message>"), "say Even the lowliest of cogs needs to say something SO SAY SOMETHING!", new[] {"broadcast"})
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
            return args.Length > 0;
        }
    }
}
