using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Originally a SubmersedVR-only fix, moved here because neither half of the underlying problem
/// is VR-specific: vanilla hides the locker/sign color-cycle button (ColorSelector, and its child
/// Indicator -- the only visible part) outside the interstitial edit state via
/// SetElementsMode(false)'s editOnly array, exposing the locker's own static base texture
/// underneath instead -- which has a plain white circle painted on it, lined up with the button's
/// position, confirmed this session via live testing and AssetRipper prefab data. That's true for
/// flat-mouse and gamepad players exactly as much as VR players; it has nothing to do with how the
/// button gets clicked. Moving it here means every player sees the true current color at all
/// times, not just SubmersedVR users, and SubmersedVR no longer needs its own duplicate copy.
/// </summary>
internal static class ColorButtonAlwaysVisibleHelper
{
    private static readonly MethodInfo UpdateColorMethod = AccessTools.Method(typeof(uGUI_SignInput), "UpdateColor");

    internal static GameObject FindColorSelector(uGUI_SignInput signInput)
    {
        foreach (Transform t in signInput.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "ColorSelector")
            {
                return t.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Order matters: reactivating a Button's GameObject fires Unity's own Selectable.OnEnable
    /// -> DoStateTransition, which immediately tints targetGraphic (Indicator) to m_NormalColor
    /// (near-white, per the prefab's Button component) -- SetActive(true) must run before
    /// UpdateColor() or that tint silently wins and undoes the color just set.
    /// </summary>
    internal static void ShowAndRefreshColor(uGUI_SignInput signInput)
    {
        GameObject colorSelector = FindColorSelector(signInput);
        if (colorSelector != null)
        {
            colorSelector.SetActive(true);
        }
        UpdateColorMethod?.Invoke(signInput, null);
    }
}

/// <summary>
/// Keeps the color button visible from the moment a locker/sign spawns, not just after the first
/// time it's selected -- SetElementsMode(false) in uGUI_SignInput.Awake deactivates ColorSelector
/// before OnSelect/OnDeselect below ever get a chance to run.
/// </summary>
public sealed partial class uGUI_SignInput_Awake_ShowColorButton_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.Method(typeof(uGUI_SignInput), "Awake");

    public static void Postfix(uGUI_SignInput __instance)
    {
        ColorButtonAlwaysVisibleHelper.ShowAndRefreshColor(__instance);
    }
}

/// <summary>
/// Re-shows and re-colors the button after vanilla's own OnDeselect body (SetElementsMode(false))
/// deactivates it -- see ColorButtonAlwaysVisibleHelper's doc comment for the full reasoning.
/// </summary>
public sealed partial class uGUI_SignInput_OnDeselect_ShowColorButton_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_SignInput t) => t.OnDeselect());

    public static void Postfix(uGUI_SignInput __instance)
    {
        ColorButtonAlwaysVisibleHelper.ShowAndRefreshColor(__instance);
    }
}

/// <summary>
/// The Label GameObject's BoxCollider (what vanilla's own physics-raycast HandTarget targeting
/// hits to trigger ColoredLabel.OnHandClick/select) doesn't extend far enough to cover where
/// ColorSelector renders -- confirmed via the prefab's own collider bounds vs. the button's
/// computed position, and via live testing: clicking directly on the now-always-visible button
/// outside edit mode fell through to whatever collider sits behind it (the locker's own
/// storage-open trigger) instead of selecting the label. Widens the *live Component* at runtime,
/// not the prefab asset on disk, to encapsulate the button's actual world-space footprint via
/// RectTransform.GetWorldCorners + Transform.InverseTransformPoint rather than hand-derived
/// local-space offsets -- this avoids repeating a world/local coordinate mistake made earlier
/// this session doing that kind of math by hand for this same button.
/// </summary>
public sealed partial class uGUI_SignInput_Awake_ExtendColliderForColorButton_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.Method(typeof(uGUI_SignInput), "Awake");

    public static void Postfix(uGUI_SignInput __instance)
    {
        GameObject colorSelector = ColorButtonAlwaysVisibleHelper.FindColorSelector(__instance);
        if (colorSelector == null)
        {
            return;
        }

        BoxCollider labelCollider = __instance.GetComponentInParent<BoxCollider>();
        RectTransform colorSelectorRect = colorSelector.GetComponent<RectTransform>();
        if (labelCollider == null || colorSelectorRect == null)
        {
            return;
        }

        Vector3[] worldCorners = new Vector3[4];
        colorSelectorRect.GetWorldCorners(worldCorners);

        Bounds bounds = new(labelCollider.center, labelCollider.size);
        foreach (Vector3 worldCorner in worldCorners)
        {
            bounds.Encapsulate(labelCollider.transform.InverseTransformPoint(worldCorner));
        }

        labelCollider.center = bounds.center;
        labelCollider.size = bounds.size;
    }
}
