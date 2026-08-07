using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
/// Makes a remote player's native off-screen chevron larger and fully opaque while preserving vanilla positioning and rotation.
/// </summary>
internal sealed class RemotePlayerPingChevron : MonoBehaviour
{
    internal const float MinimumScale = 1.75f;
    internal const float MaximumScale = 2.25f;
    internal const float PulsePeriodSeconds = 1.5f;

    private uGUI_Ping ping = null!;
    private RectTransform arrowTransform = null!;
    private Vector3 originalArrowScale;
    private bool initialized;

    private void Awake()
    {
        ping = GetComponent<uGUI_Ping>();
        arrowTransform = ping.arrow.rectTransform;
        originalArrowScale = arrowTransform.localScale;
        initialized = true;
    }

    internal static float CalculateScale(float unscaledTime)
    {
        float cycleTime = unscaledTime % PulsePeriodSeconds;
        if (cycleTime < 0f)
        {
            cycleTime += PulsePeriodSeconds;
        }

        double angle = cycleTime / PulsePeriodSeconds * Math.PI * 2d - Math.PI / 2d;
        float progress = (float)((Math.Sin(angle) + 1d) * 0.5d);
        return MinimumScale + (MaximumScale - MinimumScale) * progress;
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
        if (!initialized || !ping || !ping.arrow)
        {
            enabled = false;
            return;
        }

        if (!ping.arrow.enabled)
        {
            RestoreArrowScale();
            return;
        }

        ping.SetIconAlpha(1f);

        float scale = CalculateScale(Time.unscaledTime);
        arrowTransform.localScale = new Vector3(originalArrowScale.x * scale, originalArrowScale.y * scale, originalArrowScale.z);
    }

    private void OnDisable()
    {
        if (initialized)
        {
            ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasLast, ApplyChevronAppearance);
        }

        RestoreArrowScale();
    }

    private void RestoreArrowScale()
    {
        if (initialized && arrowTransform)
        {
            arrowTransform.localScale = originalArrowScale;
        }
    }
}
