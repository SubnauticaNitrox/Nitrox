using System.Runtime.CompilerServices;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxPatcher.Patches.Dynamic;

namespace Nitrox.Test.Client.Gui;

[TestClass]
public sealed class RemotePlayerPingChevronTest
{
    [DataTestMethod]
    [DataRow(0f, RemotePlayerPingChevron.MinimumScale)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.25f, 2f)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.5f, RemotePlayerPingChevron.MaximumScale)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds * 0.75f, 2f)]
    [DataRow(RemotePlayerPingChevron.PulsePeriodSeconds, RemotePlayerPingChevron.MinimumScale)]
    public void CalculateScaleFollowsGentlePulse(float unscaledTime, float expectedScale)
    {
        RemotePlayerPingChevron.CalculateScale(unscaledTime).Should().BeApproximately(expectedScale, 0.0001f);
    }

    [TestMethod]
    public void HudPatchResolvesInternalPlayerPingTypes()
    {
        Action initializePatch = () => RuntimeHelpers.RunClassConstructor(typeof(uGUI_Pings_OnAdd_Patch).TypeHandle);
        initializePatch.Should().NotThrow();
    }
}
