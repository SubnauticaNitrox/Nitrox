using System.Runtime.CompilerServices;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxPatcher.Patches.Dynamic;
using UnityEngine;

namespace Nitrox.Test.Client.Gui;

[TestClass]
public sealed class RemotePlayerPingChevronTest
{
    [DataTestMethod]
    [DataRow(0f, RemotePlayerPingChevron.MinimumIconScale)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.25f, 2.25f)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.5f, RemotePlayerPingChevron.MaximumIconScale)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.75f, 2.25f)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds, RemotePlayerPingChevron.MinimumIconScale)]
    public void CalculateIconScalePulsesAtOrAboveDoubleSize(float unscaledTime, float expectedScale)
    {
        RemotePlayerPingChevron.CalculateIconScale(unscaledTime).Should().BeApproximately(expectedScale, 0.0001f);
    }

    [DataTestMethod]
    [DataRow(0f, RemotePlayerPingChevron.MinimumIconScale)]
    [DataRow(RemotePlayerPingChevron.DangerPulsePeriodSeconds * 0.25f, 2.375f)]
    [DataRow(RemotePlayerPingChevron.DangerPulsePeriodSeconds * 0.5f, RemotePlayerPingChevron.DangerMaximumIconScale)]
    [DataRow(RemotePlayerPingChevron.DangerPulsePeriodSeconds * 0.75f, 2.375f)]
    [DataRow(RemotePlayerPingChevron.DangerPulsePeriodSeconds, RemotePlayerPingChevron.MinimumIconScale)]
    public void CalculateDangerIconScaleUsesFasterStrongerPulse(float unscaledTime, float expectedScale)
    {
        RemotePlayerPingChevron.CalculateDangerIconScale(unscaledTime).Should().BeApproximately(expectedScale, 0.0001f);
    }

    [DataTestMethod]
    [DataRow(false, 0f, 100f, 0f, 45f, false)]
    [DataRow(true, 25f, 100f, 45f, 45f, true)]
    [DataRow(true, 100f, 100f, 9f, 45f, true)]
    [DataRow(true, 25.01f, 100f, 9.01f, 45f, false)]
    [DataRow(true, 0f, 0f, 0f, 0f, false)]
    public void DangerRequiresSyncedCriticalHealthOrOxygen(
        bool hasReceivedVitals,
        float health,
        float maximumHealth,
        float oxygen,
        float maximumOxygen,
        bool expected)
    {
        RemotePlayerPingChevron.IsDangerous(hasReceivedVitals, health, maximumHealth, oxygen, maximumOxygen).Should().Be(expected);
    }

    [TestMethod]
    public void ChevronUsesFixedDoubleScale()
    {
        RemotePlayerPingChevron.ChevronScale.Should().Be(2f);
        RemotePlayerPingChevron.DistanceTextScale.Should().Be(2f);
    }

    [TestMethod]
    public void FullAlphaPreservesNativeTextColor()
    {
        Color nativeColor = new(0.1f, 0.2f, 0.3f, 0.4f);

        RemotePlayerPingChevron.WithFullAlpha(nativeColor).Should().Be(new Color(0.1f, 0.2f, 0.3f, 1f));
    }

    [TestMethod]
    public void HudPatchResolvesInternalPlayerPingTypes()
    {
        Action initializePatch = () => RuntimeHelpers.RunClassConstructor(typeof(uGUI_Pings_OnAdd_Patch).TypeHandle);
        initializePatch.Should().NotThrow();
    }
}
