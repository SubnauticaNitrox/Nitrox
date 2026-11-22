namespace Nitrox.Server.Subnautica.Models.Attributes;

[AttributeUsage(AttributeTargets.Class)]
internal class NameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}
