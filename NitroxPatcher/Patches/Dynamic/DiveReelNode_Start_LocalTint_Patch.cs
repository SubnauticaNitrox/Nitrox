using System.Reflection;
using NitroxClient.Extensions;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Tints the LOCAL player's own placed DiveReel (Pathfinder Tool) nodes to their own login
/// color, matching how DiveReelNodeMarkers already tints OTHER players' nodes (the base game's
/// own DiveReelNode rendering, which this patch runs alongside, otherwise always uses its
/// fixed prefab default color for the local player's own nodes).
///
/// DiveReel.CreateNewNode and DiveReelNodeMarkers.SpawnMarkerAsync both instantiate the exact
/// same prefab with no parent (confirmed against decompiled DiveReel.cs), so there is no
/// structural way to tell a real node from a decorative marker at DiveReelNode.Start() time --
/// this Harmony postfix fires for EVERY instance regardless of origin. Must check for
/// NitroxDiveReelNodeMarkerTag and skip markers entirely, or this clobbers
/// DiveReelNodeMarkers.TintMarker's own already-correct per-player tint (mesh AND light) with
/// the LOCAL player's own color -- confirmed live 2026-08-19 as every player's marker light (and
/// the directional arrow mesh) showing the local player's color instead of whoever actually
/// placed it.
///
/// DiveReelNode.Start() (decompiled reference, DiveReelNode.cs:108-134) captures ONE renderer's
/// CURRENT color (whichever is under firstNodeHolder/arrow, "useTransform") into a private
/// "baseColor" field, and Update() continuously Lerps that SAME renderer's color back toward
/// baseColor every frame (except while blinking, where it Lerps to green instead). But the node
/// has multiple separate visual pieces under it (matching DiveReelNodeMarkers.TintMarker's own
/// need to tint every MeshRenderer it finds, not just one) -- so this tints every renderer under
/// the node like TintMarker does, and only additionally overwrites baseColor (via reflection --
/// there's no public setter) for the specific renderer Update() reads back, or Update()'s Lerp
/// would fight our tint on THAT renderer back to the original default within about 1/3 second.
/// </summary>
public sealed partial class DiveReelNode_Start_LocalTint_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((DiveReelNode t) => t.Start());
    private static readonly FieldInfo BASE_COLOR_FIELD = typeof(DiveReelNode).GetField("baseColor", BindingFlags.NonPublic | BindingFlags.Instance);

    public static void Postfix(DiveReelNode __instance)
    {
        if (__instance.TryGetComponent(out NitroxDiveReelNodeMarkerTag _))
        {
            return;
        }

        Color playerColor = Resolve<LocalPlayer>().PlayerSettings.PlayerColor.ToUnity();

        foreach (MeshRenderer meshRenderer in __instance.gameObject.GetComponentsInChildren<MeshRenderer>(true))
        {
            meshRenderer.material.SetColor(ShaderPropertyID._Color, playerColor);
        }

        Transform useTransform = __instance.firstArrow ? __instance.firstNodeHolder : __instance.arrow;
        if (useTransform && useTransform.TryGetComponentInChildren(out MeshRenderer _, true))
        {
            BASE_COLOR_FIELD?.SetValue(__instance, playerColor);
        }

        if (__instance.light)
        {
            __instance.light.color = playerColor;
        }
    }
}
