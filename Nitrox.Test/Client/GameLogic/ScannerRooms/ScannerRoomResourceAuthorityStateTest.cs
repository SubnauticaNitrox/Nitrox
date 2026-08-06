using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using NitroxClient.GameLogic.ScannerRooms;

namespace Nitrox.Test.Client.GameLogic.ScannerRooms;

[TestClass]
public sealed class ScannerRoomResourceAuthorityStateTest
{
    [TestMethod]
    public void StartsPendingAndSuppressesVanillaResources()
    {
        ScannerRoomResourceAuthorityState state = new();

        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Pending);
        state.SuppressVanillaResources.Should().BeTrue();
    }

    [TestMethod]
    public void AppliedSnapshotResponseBecomesAuthoritative() =>
        AssertValidSnapshotResponseBecomesAuthoritative(ScannerRoomSnapshotApplyResult.Applied, ScannerRoomQueryStatus.Complete);

    [TestMethod]
    public void NotModifiedSnapshotResponseBecomesAuthoritative() =>
        AssertValidSnapshotResponseBecomesAuthoritative(ScannerRoomSnapshotApplyResult.NotModified, ScannerRoomQueryStatus.NotModified);

    private static void AssertValidSnapshotResponseBecomesAuthoritative(
        ScannerRoomSnapshotApplyResult result,
        ScannerRoomQueryStatus status)
    {
        ScannerRoomResourceAuthorityState state = new();

        state.ObserveAcceptedResponse(result, status).Should().BeTrue();

        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Authoritative);
        state.SuppressVanillaResources.Should().BeTrue();
    }

    [TestMethod]
    public void OnlyAcceptedRejectedFailureEnablesRollback()
    {
        ScannerRoomResourceAuthorityState state = new();

        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Ignored, ScannerRoomQueryStatus.Rejected).Should().BeFalse();
        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Failed, null).Should().BeFalse();
        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Failed).Should().BeFalse();
        state.SuppressVanillaResources.Should().BeTrue();

        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Rejected).Should().BeTrue();

        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Rollback);
        state.SuppressVanillaResources.Should().BeFalse();
    }

    [TestMethod]
    public void StateOutdatedDoesNotEnableRollback()
    {
        ScannerRoomResourceAuthorityState state = new();
        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Applied, ScannerRoomQueryStatus.Complete);

        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.StateOutdated).Should().BeFalse();

        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Authoritative);
        state.SuppressVanillaResources.Should().BeTrue();
    }

    [TestMethod]
    public void AuthoritativeResponseRecoversFromRollbackAndReconnectResetsPending()
    {
        ScannerRoomResourceAuthorityState state = new();
        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Failed, ScannerRoomQueryStatus.Rejected);

        state.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.NotModified, ScannerRoomQueryStatus.NotModified).Should().BeTrue();
        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Authoritative);
        ScannerRoomResourceAuthorityState.RequiresFallbackClear(
                ScannerRoomResourceAuthorityMode.Rollback,
                state.Mode)
            .Should().BeTrue();

        state.ResetToPending().Should().BeTrue();
        state.Mode.Should().Be(ScannerRoomResourceAuthorityMode.Pending);
        state.SuppressVanillaResources.Should().BeTrue();
    }
}
