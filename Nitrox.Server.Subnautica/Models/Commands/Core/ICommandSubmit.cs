namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal interface ICommandSubmit
{
    void ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context);
}
