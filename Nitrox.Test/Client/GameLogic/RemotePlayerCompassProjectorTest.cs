using Nitrox.Model.Core;
using NitroxClient.GameLogic.HUD;
using UnityEngine;

namespace Nitrox.Test.Client.GameLogic;

[TestClass]
public sealed class RemotePlayerCompassProjectorTest
{
    private const float ALPHA_FROM = 0.625f;
    private const float ALPHA_TO = 0.875f;

    [DataTestMethod]
    [DataRow(0f, 0f, (int)RemotePlayerCompassMarkerKind.Blip, 0f)]
    [DataRow(90f, 90f, (int)RemotePlayerCompassMarkerKind.Blip, 0f)]
    [DataRow(0f, 90f, (int)RemotePlayerCompassMarkerKind.RightArrow, 90f)]
    [DataRow(0f, 270f, (int)RemotePlayerCompassMarkerKind.LeftArrow, -90f)]
    [DataRow(350f, 10f, (int)RemotePlayerCompassMarkerKind.Blip, 20f)]
    [DataRow(10f, 350f, (int)RemotePlayerCompassMarkerKind.Blip, -20f)]
    public void ProjectsCardinalAndWrappedBearings(
        float compassHeading,
        float targetBearing,
        int expectedKind,
        float expectedSignedAngle)
    {
        bool projected = RemotePlayerCompassProjector.TryProject(
            (SessionId)1,
            Vector3.zero,
            PositionAtBearing(targetBearing, 100f),
            compassHeading,
            ALPHA_FROM,
            ALPHA_TO,
            out RemotePlayerCompassProjection result);

        projected.Should().BeTrue();
        result.MarkerKind.Should().Be((RemotePlayerCompassMarkerKind)expectedKind);
        result.TargetBearingDegrees.Should().BeApproximately(targetBearing, 0.001f);
        result.SignedAngleDegrees.Should().BeApproximately(expectedSignedAngle, 0.001f);
    }

    [TestMethod]
    public void UsesActualVisibleRangeAndChangesShapeAtBoundary()
    {
        RemotePlayerCompassProjector.TryGetVisibleHalfAngle(ALPHA_FROM, ALPHA_TO, out float halfAngle).Should().BeTrue();
        halfAngle.Should().Be(45f);

        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, PositionAtBearing(44.99f, 10f), 0f, ALPHA_FROM, ALPHA_TO, out RemotePlayerCompassProjection inside).Should().BeTrue();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, PositionAtBearing(45f, 10f), 0f, ALPHA_FROM, ALPHA_TO, out RemotePlayerCompassProjection boundary).Should().BeTrue();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, PositionAtBearing(315f, 10f), 0f, ALPHA_FROM, ALPHA_TO, out RemotePlayerCompassProjection leftBoundary).Should().BeTrue();

        inside.MarkerKind.Should().Be(RemotePlayerCompassMarkerKind.Blip);
        boundary.MarkerKind.Should().Be(RemotePlayerCompassMarkerKind.RightArrow);
        boundary.DisplayBearingDegrees.Should().Be(45f);
        leftBoundary.MarkerKind.Should().Be(RemotePlayerCompassMarkerKind.LeftArrow);
        leftBoundary.DisplayBearingDegrees.Should().Be(315f);
    }

    [TestMethod]
    public void ExactlyBehindDeterministicallyPointsRight()
    {
        RemotePlayerCompassProjector.TryProject(
            (SessionId)1,
            Vector3.zero,
            PositionAtBearing(180f, 10f),
            0f,
            ALPHA_FROM,
            ALPHA_TO,
            out RemotePlayerCompassProjection result).Should().BeTrue();

        result.SignedAngleDegrees.Should().Be(180f);
        result.MarkerKind.Should().Be(RemotePlayerCompassMarkerKind.RightArrow);
        result.DisplayBearingDegrees.Should().Be(45f);
    }

    [TestMethod]
    public void IgnoresElevationAndDoesNotLimitDistance()
    {
        Vector3 distantElevatedPlayer = PositionAtBearing(20f, 1_000_000f);
        distantElevatedPlayer.y = 50_000f;

        RemotePlayerCompassProjector.TryProject(
            (SessionId)1,
            new Vector3(0f, -2_000f, 0f),
            distantElevatedPlayer,
            0f,
            ALPHA_FROM,
            ALPHA_TO,
            out RemotePlayerCompassProjection result).Should().BeTrue();

        result.MarkerKind.Should().Be(RemotePlayerCompassMarkerKind.Blip);
        result.TargetBearingDegrees.Should().BeApproximately(20f, 0.001f);
    }

    [TestMethod]
    public void RejectsCoincidentAndInvalidInputs()
    {
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, new Vector3(0f, 100f, 0f), 0f, ALPHA_FROM, ALPHA_TO, out _).Should().BeFalse();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, new Vector3(float.NaN, 0f, 1f), 0f, ALPHA_FROM, ALPHA_TO, out _).Should().BeFalse();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, Vector3.forward, float.PositiveInfinity, ALPHA_FROM, ALPHA_TO, out _).Should().BeFalse();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, Vector3.forward, 0f, ALPHA_TO, ALPHA_FROM, out _).Should().BeFalse();
        RemotePlayerCompassProjector.TryProject((SessionId)1, Vector3.zero, Vector3.forward, 0f, float.NaN, ALPHA_TO, out _).Should().BeFalse();
    }

    private static Vector3 PositionAtBearing(float bearingDegrees, float distance)
    {
        float bearingRadians = bearingDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(bearingRadians) * distance, 0f, Mathf.Cos(bearingRadians) * distance);
    }
}
