namespace Nitrox.Test.Helper;

internal static class Extensions
{
    public static bool IsTestAssembly(this Assembly assembly)
    {
        string name = assembly.GetName().Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        if (name.EndsWith(".Test") || name.EndsWith(".Tests"))
        {
            return true;
        }
        if (name.Contains(".Test.") || name.Contains(".Tests."))
        {
            return true;
        }
        return false;
    }
}
