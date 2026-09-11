using System.Reflection;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using TMPro;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Shared broadcast logic for uGUI_SignInput_OnDeselect_Patch and
/// TMPInputField_DeactivateInputField_SignBroadcast_Patch below, plus the session-tracking flag
/// that lets OnDeselect know whether DeactivateInputField already sent this edit's final state.
/// </summary>
internal static class SignBroadcastHelper
{
    /// <summary>
    /// Set by TMPInputField_DeactivateInputField_SignBroadcast_Patch whenever it broadcasts (i.e.
    /// text was actively edited and committed this session); reset by
    /// uGUI_SignInput_OnSelect_ResetBroadcastFlag_Patch whenever a label is (re)selected. Lets
    /// uGUI_SignInput_OnDeselect_Patch skip broadcasting redundantly -- with a sign.text read
    /// directly at OnDeselect time, which carries the staleness risk documented on that patch --
    /// whenever DeactivateInputField already sent the correct, freshly-committed state.
    /// </summary>
    internal static bool TextBroadcastAlreadySentThisSession;

    internal static void BroadcastCurrentState(uGUI_SignInput sign, string logContext)
    {
        PrefabIdentifier parentIdentifier = sign.gameObject.FindAncestor<PrefabIdentifier>();
        if (!parentIdentifier)
        {
            Log.Warn($"{logContext} broadcast on {sign.gameObject.GetFullHierarchyPath()}: no PrefabIdentifier ancestor found, broadcast skipped, text=\"{sign.text}\" lost.");
            return;
        }
        if (!parentIdentifier.TryGetIdOrWarn(out NitroxId id))
        {
            Log.Warn($"{logContext} broadcast on {sign.gameObject.GetFullHierarchyPath()}: PrefabIdentifier found but no NitroxId, broadcast skipped, text=\"{sign.text}\" lost.");
            return;
        }

        EntitySignMetadata metadata = new(sign.text, sign.colorIndex, sign.scaleIndex, sign.elementsState, sign.IsBackground());
        NitroxServiceLocator.LocateService<Entities>().BroadcastMetadataUpdate(id, metadata);
    }
}

/// <summary>
/// Resets SignBroadcastHelper's session flag whenever a label is (re)selected, so
/// uGUI_SignInput_OnDeselect_Patch's own broadcast-skip logic reflects only what happened during
/// *this* edit session, not a stale flag left over from a previous one.
/// </summary>
public sealed partial class uGUI_SignInput_OnSelect_ResetBroadcastFlag_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnSelect(default));

    public static void Postfix()
    {
        SignBroadcastHelper.TextBroadcastAlreadySentThisSession = false;
    }
}

/// <summary>
/// Real broadcast trigger for edits that never touched the text field -- e.g. a click directly on
/// the color-cycle button, which changes colorIndex via ToggleColor() without ever calling
/// TMP_InputField.ActivateInputField()/DeactivateInputField(). Without this, color-only changes
/// never sync to other clients, since the only other broadcast trigger fires on text-field
/// deactivation.
///
/// Safe to read sign.text directly here specifically *because* this only runs when
/// DeactivateInputField did NOT already broadcast this session (see
/// SignBroadcastHelper.TextBroadcastAlreadySentThisSession) -- meaning the text field was never
/// activated at all during this edit, so there is no in-flight commit for OnDeselect to race
/// against. That race is real for the "text was just typed" case DeactivateInputField already
/// covers (see that patch's own doc comment), not for the "text was never touched" case this
/// method is scoped to.
/// </summary>
public sealed partial class uGUI_SignInput_OnDeselect_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnDeselect());

    public static void Postfix(uGUI_SignInput __instance)
    {
        if (SignBroadcastHelper.TextBroadcastAlreadySentThisSession)
        {
            return;
        }

        SignBroadcastHelper.BroadcastCurrentState(__instance, "OnDeselect");
    }
}

/// <summary>
/// The real sign/locker rename broadcast trigger, moved here from uGUI_SignInput.OnDeselect()
/// after confirming that method can read the text field before TMP_InputField has finished
/// committing the player's just-typed value, causing a rename to require two edits before it
/// visibly took effect for other clients. DeactivateInputField's own text parameter/state is what
/// SubmersedVR's VirtualKeyboard.cs relies on to know editing is truly done (it patches this same
/// method to clear its keyboard callback), so it's the right point to treat the field's text as
/// final.
/// </summary>
public sealed partial class TMPInputField_DeactivateInputField_SignBroadcast_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((TMP_InputField t) => t.DeactivateInputField(default));

    public static void Postfix(TMP_InputField __instance)
    {
        uGUI_SignInput sign = __instance.GetComponentInParent<uGUI_SignInput>();
        if (!sign || sign.inputField != __instance)
        {
            return;
        }

        SignBroadcastHelper.TextBroadcastAlreadySentThisSession = true;
        SignBroadcastHelper.BroadcastCurrentState(sign, "DeactivateInputField");
    }
}

/// <summary>
/// Broadcasts a color change immediately, live while still in the interstitial edit state, instead
/// of only once the player deselects, so other clients see color cycling happen in real time
/// rather than only after the fact. Safe to read sign.text here: ToggleColor() is only ever reached
/// via the color button's own click (mouse/gamepad/VR canvas raycast dispatch), never concurrently
/// with text entry, so there's no in-flight text commit to race against the way
/// DeactivateInputField's own broadcast has to guard for.
/// </summary>
public sealed partial class uGUI_SignInput_ToggleColor_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.ToggleColor());

    public static void Postfix(uGUI_SignInput __instance)
    {
        SignBroadcastHelper.BroadcastCurrentState(__instance, "ToggleColor");
    }
}
