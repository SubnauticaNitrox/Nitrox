using System.Runtime.CompilerServices;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxPatcher.Patches.Dynamic;

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

    [TestMethod]
    public void ChevronUsesFixedDoubleScale()
    {
        RemotePlayerPingChevron.ChevronScale.Should().Be(2f);
    }

    [TestMethod]
    public void HudPatchResolvesInternalPlayerPingTypes()
    {
        Action initializePatch = () => RuntimeHelpers.RunClassConstructor(typeof(uGUI_Pings_OnAdd_Patch).TypeHandle);
        initializePatch.Should().NotThrow();
    }
}
