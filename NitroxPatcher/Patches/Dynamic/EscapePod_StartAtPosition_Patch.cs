using System.Reflection;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_StartAtPosition_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((EscapePod t) => t.StartAtPosition(default));

    public static bool Prefix()
    {
        // Nitrox is responsible for choosing escape pod position see EscapePodEntitySpawner.CreateNewEscapePod()
        return false;
    }
}
