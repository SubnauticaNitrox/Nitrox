using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Nitrox.Server.Subnautica.Models.Commands.Core;

namespace Nitrox.Server.Subnautica.Models.Commands;

/// <summary>
///     Shows helpful information about available commands.
/// </summary>
[Alias("?")]
internal sealed class HelpCommand(Func<CommandRegistry> registryProvider) : ICommandHandler, ICommandHandler<string>
{
    /// <summary>
    ///     <see cref="Func{TResult}" /> is used to lazily retrieve the registry as otherwise it would cause cyclic-dependency
    ///     error if immediately requested by the <see cref="HelpCommand" /> constructor.
    /// </summary>
    private readonly Func<CommandRegistry> registryProvider = registryProvider;

    [Description("Shows this help page")]
    public async Task Execute(ICommandContext context)
    {
        StringBuilder sb = new();
        sb.AppendLine("~~~ COMMAND HELP PAGE ~~~");
        CommandRegistry registry = registryProvider();
        foreach (CommandHandlerEntry handler in registry
                                                .Handlers
                                                .OrderBy(h => h.Owner is HelpCommand ? 0 : 1)
                                                .ThenBy(h => h.Name)
                                                .ThenBy(h => h.ParameterTypes.Length))
        {
            if (!registry.IsValidHandlerForContext(handler, context))
            {
                continue;
            }
            sb.AppendLine(handler.ToString());
        }
        await context.ReplyAsync(sb.Remove(sb.Length - Environment.NewLine.Length, Environment.NewLine.Length).ToString());
    }

    [Description("Shows the help page of the given command")]
    public async Task Execute(ICommandContext context, string commandName)
    {
        if (!await TryShowHelpForCommandAsync(context, commandName))
        {
            await context.ReplyAsync($"No command exists with the name {commandName}");
        }
    }

    private async ValueTask<bool> TryShowHelpForCommandAsync(ICommandContext context, string commandName)
    {
        if (!registryProvider().TryGetHandlersByCommandName(context, commandName, out List<CommandHandlerEntry> handlers))
        {
            return false;
        }
        CommandHandlerEntry baseHandler = handlers.FirstOrDefault();
        if (baseHandler == null)
        {
            return false;
        }

        StringBuilder sb = new();
        sb.Append("~~~ ")
          .Append(baseHandler.Name.ToUpperInvariant())
          .Append(" COMMAND HAS ")
          .Append(handlers.Count)
          .Append(" HANDLER(S) ~~~");
        if (baseHandler.Aliases.Length > 0)
        {
            sb.AppendLine()
              .Append("Aliases: ")
              .AppendLine(string.Join(", ", baseHandler.Aliases));
        }
        sb.AppendLine();
        bool first = true;
        foreach (CommandHandlerEntry handler in handlers.OrderBy(h => h.Parameters.Length))
        {
            if (!first)
            {
                sb.AppendLine();
            }
            first = false;

            if (handler.Parameters.Length == 0)
            {
                sb.Append("no args - ")
                  .Append(handler.Description);
            }
            else
            {
                sb.Append(handler.ToDisplayString(false));
            }
        }
        await context.ReplyAsync(sb.ToString());
        return true;
    }
}
