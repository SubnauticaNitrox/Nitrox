namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal sealed class NopCommandSubmit : ICommandSubmit
{
    public void ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context)
    {
        // Do nothing
    }
}
