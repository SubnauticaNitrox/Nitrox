using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables local spawning of placeholders in favor of an entity creation on server-side.
/// </summary>
public sealed partial class PrefabPlaceholdersGroup_Spawn_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PrefabPlaceholdersGroup t) => t.Spawn());

    public static bool Prefix()
    {
        return false; // Disable spawning of PrefabPlaceholders(In other words large portion of objects)
    }
}
