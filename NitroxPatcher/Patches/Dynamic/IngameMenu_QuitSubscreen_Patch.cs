using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitSubscreen_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.QuitSubscreen());

    public static bool Prefix()
    {
        IngameMenu.main.QuitGame(false);
        return false;
    }
}
