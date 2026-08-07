using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Emotes;

namespace Nitrox.Test.Client.GameLogic;

[TestClass]
public sealed class EmoteWheelInputStateTest
{
    [TestMethod]
    public void ShortReleasePlaysRecentGroup()
    {
        EmoteWheelInputState state = new();
        state.Begin(10f).Should().BeTrue();

        EmoteWheelRelease release = state.Release(10f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS - 0.001f);

        release.Kind.Should().Be(EmoteWheelReleaseKind.PlayRecent);
        state.IsArmed.Should().BeFalse();
        state.IsOpen.Should().BeFalse();
    }

    [TestMethod]
    public void HoldOpensOnlyOnceAfterThreshold()
    {
        EmoteWheelInputState state = new();
        state.Begin(4f);

        state.TryOpen(4f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS - 0.001f).Should().BeFalse();
        state.TryOpen(4f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS).Should().BeTrue();
        state.TryOpen(5f).Should().BeFalse();
        state.IsOpen.Should().BeTrue();
    }

    [TestMethod]
    public void OpenWheelPlaysSelectedGroupOnRelease()
    {
        EmoteWheelInputState state = new();
        state.Begin(1f);
        state.TryOpen(1f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS);
        state.SetSelection(PlayerEmoteGroup.TeamUp);

        EmoteWheelRelease release = state.Release(2f);

        release.Kind.Should().Be(EmoteWheelReleaseKind.PlaySelected);
        release.SelectedGroup.Should().Be(PlayerEmoteGroup.TeamUp);
    }

    [TestMethod]
    public void CenterReleaseCancels()
    {
        EmoteWheelInputState state = new();
        state.Begin(1f);
        state.TryOpen(1f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS);
        state.SetSelection(null);

        state.Release(2f).Kind.Should().Be(EmoteWheelReleaseKind.None);
    }

    [TestMethod]
    public void CancelClearsArmedAndOpenStates()
    {
        EmoteWheelInputState state = new();
        state.Begin(1f);
        state.TryOpen(1f + EmoteWheelInputState.HOLD_THRESHOLD_SECONDS);
        state.SetSelection(PlayerEmoteGroup.No);

        state.Cancel();

        state.IsArmed.Should().BeFalse();
        state.IsOpen.Should().BeFalse();
        state.Release(2f).Kind.Should().Be(EmoteWheelReleaseKind.None);
    }
}
