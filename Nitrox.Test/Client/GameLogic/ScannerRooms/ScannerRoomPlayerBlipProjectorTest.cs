using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.GameLogic.ScannerRooms;

[TestClass]
public class ScannerRoomPlayerBlipProjectorTest
{
    [TestMethod]
    public void IncludesPlayerExactlyOnRangeBoundary()
    {
        SessionId sessionId = 1;
        ScannerRoomPlayerLocation player = new(sessionId, "Boundary", new Vector3(300f, 0f, 0f));

        IReadOnlyList<ScannerRoomPlayerBlip> result = ScannerRoomPlayerBlipProjector.Project(
            [player],
            Vector3.zero,
            Vector3.zero,
            300f,
            0.01f);

        result.Should().ContainSingle();
        result[0].SessionId.Should().Be(sessionId);
        result[0].LocalPosition.x.Should().BeApproximately(3f, 0.0001f);
    }

    [TestMethod]
    public void ExcludesPlayerOutsideRange()
    {
        ScannerRoomPlayerLocation player = new((SessionId)1, "Outside", new Vector3(300.01f, 0f, 0f));

        IReadOnlyList<ScannerRoomPlayerBlip> result = ScannerRoomPlayerBlipProjector.Project(
            [player],
            Vector3.zero,
            Vector3.zero,
            300f,
            0.01f);

        result.Should().BeEmpty();
    }

    [TestMethod]
    public void ProjectsRelativeToMapOrigin()
    {
        ScannerRoomPlayerLocation player = new((SessionId)1, "Mapped", new Vector3(20f, 30f, 40f));

        IReadOnlyList<ScannerRoomPlayerBlip> result = ScannerRoomPlayerBlipProjector.Project(
            [player],
            new Vector3(10f, 20f, 30f),
            new Vector3(5f, 10f, 15f),
            100f,
            0.1f);

        result.Should().ContainSingle();
        result[0].LocalPosition.x.Should().BeApproximately(1.5f, 0.0001f);
        result[0].LocalPosition.y.Should().BeApproximately(2f, 0.0001f);
        result[0].LocalPosition.z.Should().BeApproximately(2.5f, 0.0001f);
    }

    [TestMethod]
    public void DeduplicatesAndOrdersPlayersBySessionId()
    {
        ScannerRoomPlayerLocation firstVersion = new((SessionId)2, "Old name", new Vector3(1f, 0f, 0f));
        ScannerRoomPlayerLocation lowerSession = new((SessionId)1, "First", new Vector3(2f, 0f, 0f));
        ScannerRoomPlayerLocation latestVersion = new((SessionId)2, "Latest name", new Vector3(3f, 0f, 0f));

        IReadOnlyList<ScannerRoomPlayerBlip> result = ScannerRoomPlayerBlipProjector.Project(
            [firstVersion, lowerSession, latestVersion],
            Vector3.zero,
            Vector3.zero,
            300f,
            1f);

        result.Select(player => (ushort)player.SessionId).Should().Equal(1, 2);
        result[1].PlayerName.Should().Be("Latest name");
        result[1].LocalPosition.x.Should().Be(3f);
    }

    [TestMethod]
    public void RejectsInvalidProjectionInputs()
    {
        ScannerRoomPlayerLocation player = new((SessionId)1, "Invalid", Vector3.zero);

        ScannerRoomPlayerBlipProjector.Project([player], Vector3.zero, Vector3.zero, -1f, 0.01f).Should().BeEmpty();
        ScannerRoomPlayerBlipProjector.Project([player], Vector3.zero, Vector3.zero, 300f, float.NaN).Should().BeEmpty();
        ScannerRoomPlayerBlipProjector.Project([player], new Vector3(float.PositiveInfinity, 0f, 0f), Vector3.zero, 300f, 0.01f).Should().BeEmpty();
    }
}
