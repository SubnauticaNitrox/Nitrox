using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class IngameMenu_QuitGame_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((IngameMenu t) => t.QuitGame(default(bool)));

    public static bool Prefix()
    {
        // TODO: Remove this patch after fixing that no MP resources are left on disconnect. So that we can return to main menu.
        Application.Quit();
        return false;
    }
}
