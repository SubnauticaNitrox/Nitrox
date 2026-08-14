using Nitrox.Model.Core;
using NitroxClient.GameLogic.HUD;
using UnityEngine;

namespace Nitrox.Test.Client.GameLogic;

[TestClass]
public sealed class RemotePlayerCompassMarkerLayoutTest
{
    [TestMethod]
    public void StacksCollidingBlipsInSessionOrder()
    {
        IReadOnlyList<RemotePlayerCompassLayoutResult> result = RemotePlayerCompassMarkerLayout.Arrange(
        [
            new RemotePlayerCompassLayoutInput((SessionId)2, RemotePlayerCompassMarkerKind.Blip, new Vector2(10f, 7f)),
            new RemotePlayerCompassLayoutInput((SessionId)1, RemotePlayerCompassMarkerKind.Blip, new Vector2(0f, 3f))
        ]);

        result.Select(marker => (ushort)marker.SessionId).Should().Equal(1, 2);
        result[0].Position.Should().Be(new Vector2(0f, 0f));
        result[1].Position.Should().Be(new Vector2(10f, 10f));
    }

    [TestMethod]
    public void DoesNotStackBlipsOutsideCollisionWidth()
    {
        IReadOnlyList<RemotePlayerCompassLayoutResult> result = RemotePlayerCompassMarkerLayout.Arrange(
        [
            new RemotePlayerCompassLayoutInput((SessionId)1, RemotePlayerCompassMarkerKind.Blip, Vector2.zero),
            new RemotePlayerCompassLayoutInput((SessionId)2, RemotePlayerCompassMarkerKind.Blip, new Vector2(RemotePlayerCompassMarkerLayout.CollisionWidth + 0.01f, 0f))
        ]);

        result[0].Position.y.Should().Be(0f);
        result[1].Position.y.Should().Be(0f);
    }

    [TestMethod]
    public void StacksAllArrowsOnEachSideWithoutMixingSides()
    {
        IReadOnlyList<RemotePlayerCompassLayoutResult> result = RemotePlayerCompassMarkerLayout.Arrange(
        [
            new RemotePlayerCompassLayoutInput((SessionId)3, RemotePlayerCompassMarkerKind.RightArrow, new Vector2(100f, 2f)),
            new RemotePlayerCompassLayoutInput((SessionId)2, RemotePlayerCompassMarkerKind.LeftArrow, new Vector2(-100f, 2f)),
            new RemotePlayerCompassLayoutInput((SessionId)1, RemotePlayerCompassMarkerKind.LeftArrow, new Vector2(-100f, 2f))
        ]);

        result.Single(marker => (ushort)marker.SessionId == 1).Position.y.Should().Be(-3f);
        result.Single(marker => (ushort)marker.SessionId == 2).Position.y.Should().Be(7f);
        result.Single(marker => (ushort)marker.SessionId == 3).Position.y.Should().Be(2f);
    }

    [TestMethod]
    public void FiltersInvalidProjectedPositions()
    {
        IReadOnlyList<RemotePlayerCompassLayoutResult> result = RemotePlayerCompassMarkerLayout.Arrange(
        [
            new RemotePlayerCompassLayoutInput((SessionId)1, RemotePlayerCompassMarkerKind.Blip, Vector2.zero),
            new RemotePlayerCompassLayoutInput((SessionId)2, RemotePlayerCompassMarkerKind.Blip, new Vector2(float.NaN, 0f))
        ]);

        result.Should().ContainSingle();
        result[0].SessionId.Should().Be((SessionId)1);
    }
}
