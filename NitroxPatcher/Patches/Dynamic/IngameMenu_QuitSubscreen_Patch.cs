using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitSubscreen_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((IngameMenu t) => t.QuitSubscreen());

    public static bool Prefix()
    {
        IngameMenu.main.QuitGame(false);
        return false;
    }
}
