using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class HelpCommand : Command
    {
        public HelpCommand() : base("help", Perms.PLAYER, "", "Displays this", new[] { "?" })
        {
        }

        public override void RunCommand(string[] args, Optional<Player> sender)
        {
            if (sender.HasValue)
            {
                List<string> cmdsText = GetHelpText(sender.Value.Permissions);
                cmdsText.ForEach(cmdText => SendMessageToPlayer(sender, cmdText));
            }
            else
            {
                List<string> cmdsText = GetHelpText(Perms.CONSOLE);
                cmdsText.ForEach(cmdText => Log.Info(cmdText));
            }
        }

        public override bool VerifyArgs(string[] args)
        {
            return args.Length == 0;
        }

        private List<string> GetHelpText(Perms perm)
        {
            // runtime query to avoid circular dependencies
            IEnumerable<Command> commands = NitroxModel.Core.NitroxServiceLocator.LocateService<IEnumerable<Command>>();
            return new List<string>(commands.Where(cmd => cmd.RequiredPermLevel <= perm)
                                            .OrderByDescending(cmd => cmd.Name)
                                            .Select(cmd => cmd.ToHelpText()));
        }
    }
}
