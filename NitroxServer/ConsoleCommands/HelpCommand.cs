using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class HelpCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "?" };

        public HelpCommand() : base("help", Perms.PLAYER, "Displays this")
        {
            AllowedArgOverflow = true;
        }

        protected override void Execute(CallArgs args)
        {
            List<string> cmdsText;

            if (args.IsConsole)
            {
                cmdsText = GetHelpText(Perms.CONSOLE, false);

                foreach (string cmdText in cmdsText)
                {
                    Log.Info(cmdText);
                }
            }
            else
            {
                cmdsText = GetHelpText(args.Sender.Value.Permissions, true);

                foreach (string cmdText in cmdsText)
                {
                    SendMessageToPlayer(args.Sender, cmdText);
                }
            }
        }

        private List<string> GetHelpText(Perms permThreshold, bool cropText)
        {
            //Runtime query to avoid circular dependencies
            IEnumerable<Command> commands = NitroxServiceLocator.LocateService<IEnumerable<Command>>();
            return new List<string>(commands.Where(cmd => cmd.CanExecute(permThreshold))
                                            .OrderByDescending(cmd => cmd.Name)
                                            .Select(cmd => cmd.ToHelpText(cropText)));
        }
    }
}
