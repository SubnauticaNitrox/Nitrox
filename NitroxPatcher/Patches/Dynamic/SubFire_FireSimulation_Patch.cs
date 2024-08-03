# if DEBUG
using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables cyclops overheating from moving while in creative mode (really annoying for no reason)
/// </summary>
public sealed partial class SubFire_FireSimulation_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubFire t) => t.FireSimulation());

    public static bool Prefix() => SubFire_EngineOverheatSimulation_Patch.Prefix();
}
#endif
