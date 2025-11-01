using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxPatcher.Helper;

internal static class SupportHelper
{
    /// <summary>
    ///     Dependency names commonly used by Unity game mods.
    /// </summary>
    private static readonly string[] modDependencies = ["HarmonyX", "0Harmony", "Harmony", "MonoMod.RuntimeDetour"];

    /// <summary>
    ///     Logs a list of all other mods that are currently loaded.
    /// </summary>
    public static string? GetSummaryOfOtherMods()
    {
        Assembly[] otherMods = GetOtherMods().ToArray();
        if (otherMods.Length < 1)
        {
            return null;
        }
        StringBuilder sb = new();
        sb.AppendLine("Nitrox has detected other mods! Please do not report Nitrox issues while mods are loaded!");
        foreach (Assembly mod in otherMods)
        {
            AssemblyName modNameInfo = mod.GetName();
            sb.Append(modNameInfo.Name)
              .Append(" v")
              .Append(modNameInfo.Version)
              .Append(" path '")
              .Append(RedactPath(mod.Location))
              .AppendLine("'");
        }
        return sb.ToString();

        static IEnumerable<Assembly> GetOtherMods()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (IsNotNitroxMod(assembly))
                {
                    yield return assembly;
                }
            }
        }

        static bool IsNotNitroxMod(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            if (name.StartsWith("Nitrox"))
            {
                return false;
            }
            // Exclude assemblies that are the mod dependencies themselves as they don't do anything that can conflict with Nitrox.
            if (modDependencies.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }
            foreach (AssemblyName dependency in assembly.GetReferencedAssemblies())
            {
                if (modDependencies.Contains(dependency.Name, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        static string RedactPath(string path)
        {
            int trimEnd = path.LastIndexOf($"{Path.DirectorySeparatorChar}BepInEx{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
            if (trimEnd < 0)
            {
                return path;
            }
            return $"<GAME_ROOT>{path.Substring(trimEnd)}";
        }
    }
}
