using System.Collections.Generic;
using System.Linq;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxServer.ConsoleCommands.Abstract;
using NitroxServer.ConsoleCommands.Abstract.Type;

namespace NitroxServer.ConsoleCommands
{
    internal class HelpCommand : Command
    {
        public override IEnumerable<string> Aliases { get; } = new[] { "?" };

        public HelpCommand() : base("help", Perms.PLAYER, "Displays this")
        {
            AddParameter(new TypeString("command", false, "Command to see help information for"));
        }

        protected override void Execute(CallArgs args)
        {
            List<string> cmdsText = new();
            cmdsText.Add(args.IsValid(0) ? $"=== Showing help for {args.Get<string>(0)} ===" : "=== Showing command list ===");
            if (args.IsConsole)
            {
                cmdsText.AddRange(GetHelpText(Perms.CONSOLE, false, args.IsValid(0) ? args.Get<string>(0) : null));

                foreach (string cmdText in cmdsText)
                {
                    Log.Info(cmdText);
                }
            }
            else
            {
                cmdsText.AddRange(GetHelpText(args.Sender.Value.Permissions, true, args.IsValid(0) ? args.Get<string>(0) : null));

                foreach (string cmdText in cmdsText)
                {
                    SendMessageToPlayer(args.Sender, cmdText);
                }
            }
        }

        private List<string> GetHelpText(Perms permThreshold, bool cropText, string singleCommand)
        {
            //Runtime query to avoid circular dependencies
            IEnumerable<Command> commands = NitroxServiceLocator.LocateService<IEnumerable<Command>>();
            if (singleCommand != null && !commands.Any(cmd => cmd.Name.Equals(singleCommand)))
            {
                return new List<string>() { "Command does not exist" };
            }
            return new List<string>(commands.Where(cmd => cmd.CanExecute(permThreshold) && (singleCommand == null || cmd.Name.Equals(singleCommand)))
                                            .OrderByDescending(cmd => cmd.Name)
                                            .Select(cmd => cmd.ToHelpText(singleCommand != null, cropText)));
        }
    }
}
