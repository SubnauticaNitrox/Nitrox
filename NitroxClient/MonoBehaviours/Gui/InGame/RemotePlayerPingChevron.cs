using System;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel;
using NitroxClient.MonoBehaviours.Gui.HUD;
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
    internal const float DangerMaximumIconScale = 2.75f;
    internal const float DangerPulsePeriodSeconds = 0.6f;
    internal const float CriticalHealthFraction = 0.25f;
    internal const float CriticalOxygenFraction = 0.2f;
    private const float PlayerMaximumHealth = 100f;

    private uGUI_Ping ping = null!;
    private RemotePlayerPingIdentifier remotePlayerIdentifier = null!;
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
        return CalculatePulseScale(unscaledTime, PulsePeriodSeconds, MinimumIconScale, MaximumIconScale);
    }

    internal static float CalculateDangerIconScale(float unscaledTime)
    {
        return CalculatePulseScale(unscaledTime, DangerPulsePeriodSeconds, MinimumIconScale, DangerMaximumIconScale);
    }

    internal static bool IsDangerous(bool hasReceivedVitals, float health, float maximumHealth, float oxygen, float maximumOxygen)
    {
        return hasReceivedVitals &&
               (IsCritical(health, maximumHealth, CriticalHealthFraction) ||
                IsCritical(oxygen, maximumOxygen, CriticalOxygenFraction));
    }

    internal void Configure(Component identifier)
    {
        RestoreIndicatorScale();
        remotePlayerIdentifier = identifier as RemotePlayerPingIdentifier;
    }

    private static float CalculatePulseScale(float unscaledTime, float period, float minimumScale, float maximumScale)
    {
        float cycleTime = unscaledTime % period;
        if (cycleTime < 0f)
        {
            cycleTime += period;
        }

        double angle = cycleTime / period * Math.PI * 2d - Math.PI / 2d;
        float progress = (float)((Math.Sin(angle) + 1d) * 0.5d);
        return minimumScale + (maximumScale - minimumScale) * progress;
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
        float iconScale = IsRemotePlayerInDanger()
            ? CalculateDangerIconScale(Time.unscaledTime)
            : CalculateIconScale(Time.unscaledTime);
        iconTransform.localScale = Scale2D(originalIconScale, iconScale);
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

    private bool IsRemotePlayerInDanger()
    {
        if (!remotePlayerIdentifier || remotePlayerIdentifier.Player is not RemotePlayer remotePlayer || !remotePlayer.vitals)
        {
            return false;
        }

        RemotePlayerVitals vitals = remotePlayer.vitals;
        return IsDangerous(vitals.HasReceivedVitals, vitals.CurrentHealth, PlayerMaximumHealth, vitals.CurrentOxygen, vitals.MaximumOxygen);
    }

    private static bool IsCritical(float current, float maximum, float criticalFraction) =>
        maximum > 0f && current / maximum <= criticalFraction;
}
