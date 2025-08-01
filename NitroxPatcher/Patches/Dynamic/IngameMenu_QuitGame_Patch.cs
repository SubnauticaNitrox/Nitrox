using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitGame_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((IngameMenu t) => t.QuitGame(default(bool)));

    public static bool Prefix()
    {
        return true;
    }
}
