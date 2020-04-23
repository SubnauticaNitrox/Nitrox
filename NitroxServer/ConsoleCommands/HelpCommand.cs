using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxServer.ConsoleCommands.Abstract;

namespace NitroxServer.ConsoleCommands
{
    internal class HelpCommand : Command
    {
        public HelpCommand() : base("help", Perms.PLAYER, "Displays this", true)
        {
            AddAlias("?");
        }

        protected override void Execute(CallArgs args)
        {
            if (args.Sender.HasValue)
            {
                List<string> cmdsText = GetHelpText(args.Sender.Value.Permissions, true);
                cmdsText.ForEach(cmdText => SendMessageToPlayer(args.Sender, cmdText));
            }
            else
            {
                List<string> cmdsText = GetHelpText(Perms.CONSOLE, false);
                foreach (string cmdText in cmdsText)
                {
                    Log.Info(cmdText);
                }
            }
        }

        private List<string> GetHelpText(Perms permThreshold, bool cropText)
        {
            //Runtime query to avoid circular dependencies
            IEnumerable<Command> commands = NitroxServiceLocator.LocateService<IEnumerable<Command>>();
            return new List<string>(commands.Where(cmd => cmd.RequiredPermLevel <= permThreshold)
                                            .OrderByDescending(cmd => cmd.Name)
                                            .Select(cmd => cmd.ToHelpText(cropText)));
        }
    }
}
