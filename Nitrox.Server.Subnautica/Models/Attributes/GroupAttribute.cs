namespace Nitrox.Server.Subnautica.Models.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class GroupAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}
