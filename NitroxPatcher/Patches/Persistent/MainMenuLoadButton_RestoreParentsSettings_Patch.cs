using System.Reflection;
using NitroxModel.Helper;
using UnityEngine.UI;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// MainMenuLoadButton.RestoreParentsSettings() is throwing null refs because we copy it for our MainMenu and then destroy it.
/// Unfortunately OnDestroy can't be prevented when destroying MBs so we fix the NRE here.
/// </summary>
public sealed partial class MainMenuLoadButton_RestoreParentsSettings_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((MainMenuLoadButton lb) => lb.RestoreParentsSettings());

    public static bool Prefix(GridLayoutGroup ___gridLayoutGroup, ScrollRect ___scrollRect)
    {
        return ___gridLayoutGroup && ___scrollRect;
    }
}
