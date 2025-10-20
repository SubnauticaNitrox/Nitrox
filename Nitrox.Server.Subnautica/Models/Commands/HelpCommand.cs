using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Commands.Abstract.Type;

namespace Nitrox.Server.Subnautica.Models.Commands
{
    internal class HelpCommand : Command
    {
        private readonly ILogger<HelpCommand> logger;
        public override IEnumerable<string> Aliases { get; } = new[] { "?" };

        public HelpCommand(ILogger<HelpCommand> logger) : base("help", Perms.PLAYER, "Displays this")
        {
            this.logger = logger;
            AddParameter(new TypeString("command", false, "Command to see help information for"));
        }

        protected override void Execute(CallArgs args)
        {
            List<string> cmdsText;
            if (args.IsConsole)
            {
                cmdsText = GetHelpText(Perms.HOST, false, args.IsValid(0) ? args.Get<string>(0) : null);
                using (logger.BeginPlainScope())
                {
                    foreach (string cmdText in cmdsText)
                    {
                        logger.ZLogInformation($"{cmdText}");
                    }
                }
            }
            else
            {
                cmdsText = GetHelpText(args.Sender.Value.Permissions, true, args.IsValid(0) ? args.Get<string>(0) : null);

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
                return ["Command does not exist"];
            }
            List<string> cmdsText = [];
            cmdsText.Add(singleCommand != null ? $"=== Showing help for {singleCommand} ===" : "=== Showing command list ===");
            cmdsText.AddRange(commands.Where(cmd => CanExecuteAndProcess(cmd, permThreshold) && (singleCommand == null || cmd.Name.Equals(singleCommand)))
                                             .OrderByDescending(cmd => cmd.Name)
                                             .Select(cmd => cmd.ToHelpText(singleCommand != null, cropText)));
            return cmdsText;

            static bool CanExecuteAndProcess(Command cmd, Perms perms)
            {
                return cmd.CanExecute(perms) && !(perms == Perms.HOST && cmd.Flags.HasFlag(PermsFlag.NO_CONSOLE));
            }
        }
    }
}
