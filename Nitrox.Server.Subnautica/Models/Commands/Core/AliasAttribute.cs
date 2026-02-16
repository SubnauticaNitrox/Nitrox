namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[AttributeUsage(AttributeTargets.Class)]
internal sealed class AliasAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; } = aliases;
}
