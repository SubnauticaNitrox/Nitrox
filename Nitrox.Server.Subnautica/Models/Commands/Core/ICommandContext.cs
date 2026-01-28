using System.Diagnostics.CodeAnalysis;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

/// <summary>
///     A context that can be provided to a command as it is called.
/// </summary>
internal interface ICommandContext
{
    public ILogger Logger { get; set; }

    CommandOrigin Origin { get; init; }

    public string OriginName { get; }

    SessionId OriginId { get; init; }

    /// <summary>
    ///     The permissions of the issuer as they were when the command was issued.
    /// </summary>
    Perms Permissions { get; init; }

    /// <summary>
    ///     Sends a message back to the command issuer.
    /// </summary>
    /// <param name="data">The data to send.</param>
    Task ReplyAsync<T>(T data);

    ValueTask SendAsync<T>(SessionId sessionId, T data);
    ValueTask SendToAllAsync<T>(T data);
    ValueTask SendToOthersAsync<T>(T data);

    [DoesNotReturn]
    static void ThrowNotSupportedData<T>(T data)
    {
        throw new NotSupportedException($"Unsupported data type {data?.GetType()} with value: {data?.ToString() ?? "<null>"}");
    }
}
