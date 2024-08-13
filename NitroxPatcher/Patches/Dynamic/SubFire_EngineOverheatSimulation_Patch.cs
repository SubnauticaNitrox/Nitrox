# if DEBUG
using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disables cyclops overheating from moving while in creative mode (really annoying for no reason)
/// </summary>
public sealed partial class SubFire_EngineOverheatSimulation_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubFire t) => t.EngineOverheatSimulation());

    public static bool Prefix()
    {
        if ((GameModeUtils.currentGameMode & GameModeOption.Creative) != 0)
        {
            return false;
        }
        return true;
    }
}
#endif
