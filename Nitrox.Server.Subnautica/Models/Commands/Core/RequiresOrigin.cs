namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class RequiresOrigin(CommandOrigin acceptedOrigin) : Attribute
{
    /// <summary>
    ///     Gets the accepted origin for this command. Commands not part of the issuer origin will be hidden and blocked.
    /// </summary>
    public CommandOrigin AcceptedOrigin { get; } = acceptedOrigin;
}
