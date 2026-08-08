using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
/// Makes a remote player's native off-screen icon and chevron larger and fully opaque while preserving vanilla positioning and rotation.
/// </summary>
internal sealed class RemotePlayerPingChevron : MonoBehaviour
{
    internal const float ChevronScale = 2f;
    internal const float MinimumIconScale = 2f;
    internal const float MaximumIconScale = 2.5f;
    internal const float PulsePeriodSeconds = 1.5f;

    private uGUI_Ping ping = null!;
    private RectTransform arrowTransform = null!;
    private RectTransform iconTransform = null!;
    private Vector3 originalArrowScale;
    private Vector3 originalIconScale;
    private bool initialized;

    private void Awake()
    {
        ping = GetComponent<uGUI_Ping>();
        arrowTransform = ping.arrow.rectTransform;
        iconTransform = ping.icon.rectTransform;
        originalArrowScale = arrowTransform.localScale;
        originalIconScale = iconTransform.localScale;
        initialized = true;
    }

    internal static float CalculateIconScale(float unscaledTime)
    {
        float cycleTime = unscaledTime % PulsePeriodSeconds;
        if (cycleTime < 0f)
        {
            cycleTime += PulsePeriodSeconds;
        }

        double angle = cycleTime / PulsePeriodSeconds * Math.PI * 2d - Math.PI / 2d;
        float progress = (float)((Math.Sin(angle) + 1d) * 0.5d);
        return MinimumIconScale + (MaximumIconScale - MinimumIconScale) * progress;
    }

    private void OnEnable()
    {
        if (initialized)
        {
            ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasLast, ApplyChevronAppearance);
        }
    }

    private void ApplyChevronAppearance()
    {
        if (!initialized || !ping || !ping.arrow || !ping.icon)
        {
            enabled = false;
            return;
        }

        if (!ping.arrow.enabled)
        {
            RestoreIndicatorScale();
            return;
        }

        ping.SetIconAlpha(1f);
        arrowTransform.localScale = Scale2D(originalArrowScale, ChevronScale);
        iconTransform.localScale = Scale2D(originalIconScale, CalculateIconScale(Time.unscaledTime));
    }

    private void OnDisable()
    {
        if (initialized)
        {
            ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasLast, ApplyChevronAppearance);
        }

        RestoreIndicatorScale();
    }

    private void RestoreIndicatorScale()
    {
        if (initialized && arrowTransform)
        {
            arrowTransform.localScale = originalArrowScale;
        }

        if (initialized && iconTransform)
        {
            iconTransform.localScale = originalIconScale;
        }
    }

    private static Vector3 Scale2D(Vector3 originalScale, float scale) =>
        new(originalScale.x * scale, originalScale.y * scale, originalScale.z);
}
