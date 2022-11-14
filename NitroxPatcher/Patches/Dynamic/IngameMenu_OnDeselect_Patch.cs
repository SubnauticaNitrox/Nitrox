using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Disable the possibility of escaping a modal by clicking outside of its box or pressing the escape for the modals that want it
/// </summary>
public class IngameMenu_OnDeselect_Patch : NitroxPatch, IDynamicPatch
{
    private static MethodInfo TARGET_METHOD = Reflect.Method((IngameMenu t) => t.OnDeselect());

    // OnDeselect happens when you deselect the modal (by clicking outside or pressing escape)
    // Therefore, if we prevent it from happening it certain cases, it will force the user to click one of the buttons of the modal
    public static bool Prefix()
    {
        // Cancel if the modal is set to be non-avoidable (IsAvoidable) and if it's not the modal that wants to close itself (IsAvoidableBypass)
        if (Modal.CurrentModal != null && !Modal.CurrentModal.IsAvoidable && !Modal.CurrentModal.IsAvoidableBypass)
        {
            return false;
        }
        if (Modal.CurrentModal != null)
        {
            Modal.CurrentModal.OnDeselect();
        }
        Modal.CurrentModal = null;
        return true;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
