using System;

namespace Nitrox.Server.Subnautica.Models.Commands.Core;

[AttributeUsage(AttributeTargets.Class)]
public class AliasAttribute(params string[] aliases) : Attribute
{
    public string[] Aliases { get; } = aliases;
}
