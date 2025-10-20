using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Commands.Abstract;
using Nitrox.Server.Subnautica.Models.Exceptions;

namespace Nitrox.Server.Subnautica.Models.Commands.Processor
{
    public class TextCommandProcessor
    {
        private readonly ILogger<TextCommandProcessor> logger;
        private readonly Dictionary<string, Command> commands = new();
        private readonly char[] splitChar = [' '];

        public TextCommandProcessor(IEnumerable<Command> cmds, ILogger<TextCommandProcessor> logger)
        {
            this.logger = logger;
            foreach (Command cmd in cmds)
            {
                if (!commands.TryAdd(cmd.Name, cmd))
                {
                    throw new DuplicateRegistrationException($"Command {cmd.Name} is registered multiple times.");
                }

                foreach (string alias in cmd.Aliases)
                {
                    if (!commands.TryAdd(alias, cmd))
                    {
                        throw new DuplicateRegistrationException($"Command {alias} is registered multiple times.");
                    }
                }
            }
            if (commands.Count < 1)
            {
                logger.ZLogWarning($"No commands registered");
            }
        }

        public void ProcessCommand(string msg, Optional<Player> sender, Perms permissions)
        {
            if (string.IsNullOrWhiteSpace(msg))
            {
                return;
            }
            Span<string> parts = msg.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            if (!commands.TryGetValue(parts[0], out Command command))
            {
                Command.SendMessage(sender, $"Command not found: {parts[0]}");
                return;
            }
            if (!sender.HasValue && command.Flags.HasFlag(PermsFlag.NO_CONSOLE))
            {
                logger.ZLogError($"This command cannot be used by CONSOLE");
                return;
            }

            if (command.CanExecute(permissions))
            {
                try
                {
                    command.TryExecute(sender, parts[1..]);

                }
                catch (ArgumentException ex)
                {
                    Command.SendMessage(sender, $"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    logger.ZLogError(ex, $"Fatal error while trying to execute the command");
                }
            }
            else
            {
                Command.SendMessage(sender, "You do not have the required permissions for this command !");
            }
        }
    }
}
