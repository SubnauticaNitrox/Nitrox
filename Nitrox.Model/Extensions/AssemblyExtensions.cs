using System.Reflection;

namespace Nitrox.Model.Extensions;

public static class AssemblyExtensions
{
    public static string? GetMetaData(this Assembly assembly, string key)
    {
        AssemblyMetadataAttribute first = null;
        foreach (AssemblyMetadataAttribute attr in assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
        {
            if (attr.Key == key)
            {
                first = attr;
                break;
            }
        }
        return first?.Value;
    }
}
