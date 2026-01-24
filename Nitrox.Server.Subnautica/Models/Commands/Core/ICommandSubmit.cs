namespace Nitrox.Server.Subnautica.Models.Commands.Core;

internal interface ICommandSubmit
{
    /// <summary>
    ///     Tries to execute the command that matches the input.
    /// </summary>
    /// <param name="inputText">The text input which should be interpreted as a command.</param>
    /// <param name="context">The context that should be given to the command handler if found.</param>
    /// <param name="commandTask">Task that is set if the command is async.</param>
    bool ExecuteCommand(ReadOnlySpan<char> inputText, ICommandContext context, out Task<bool>? commandTask);
}
