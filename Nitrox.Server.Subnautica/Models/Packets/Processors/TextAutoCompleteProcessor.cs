using System.Text.RegularExpressions;
using Nitrox.Server.Subnautica.Models.Commands.Core;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     Provides contextual auto completion to clients upon request.
/// </summary>
internal sealed class TextAutoCompleteProcessor(CommandRegistry commandRegistry) : IAuthPacketProcessor<TextAutoComplete>
{
    private readonly CommandRegistry commandRegistry = commandRegistry;

    public async Task Process(AuthProcessorContext context, TextAutoComplete packet)
    {
        switch (packet.Context)
        {
            case TextAutoComplete.AutoCompleteContext.COMMAND_NAME:
                Regex matchCommandNameRegex = new($@"^{packet.Text}\w+$", RegexOptions.NonBacktracking | RegexOptions.ExplicitCapture | RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
                string? commandName = commandRegistry.FindCommandName(matchCommandNameRegex, context.Sender.Permissions);
                if (string.IsNullOrWhiteSpace(commandName))
                {
                    break;
                }
                await context.ReplyAsync(new TextAutoComplete(commandName, TextAutoComplete.AutoCompleteContext.COMMAND_NAME));
                break;
        }
    }
}
