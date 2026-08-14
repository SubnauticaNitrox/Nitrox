using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

internal enum RemotePlayerCompassMarkerKind
{
    Blip,
    LeftArrow,
    RightArrow
}

internal readonly record struct RemotePlayerCompassProjection(
    SessionId SessionId,
    RemotePlayerCompassMarkerKind MarkerKind,
    float TargetBearingDegrees,
    float DisplayBearingDegrees,
    float SignedAngleDegrees);

/// <summary>
/// Projects a remote player's horizontal world position into the heading range represented by the vanilla compass.
/// Unity object lifecycle and UI placement are deliberately kept out of this class so boundary behavior stays testable.
/// </summary>
internal static class RemotePlayerCompassProjector
{
    private const float MINIMUM_HORIZONTAL_DISTANCE_SQUARED = 0.000001f;

    public static bool TryProject(
        SessionId sessionId,
        Vector3 observerWorldPosition,
        Vector3 playerWorldPosition,
        float compassHeadingDegrees,
        float compassAlphaFrom,
        float compassAlphaTo,
        out RemotePlayerCompassProjection projection)
    {
        projection = default;

        if (!IsFinite(observerWorldPosition) ||
            !IsFinite(playerWorldPosition) ||
            !IsFinite(compassHeadingDegrees) ||
            !TryGetVisibleHalfAngle(compassAlphaFrom, compassAlphaTo, out float visibleHalfAngleDegrees))
        {
            return false;
        }

        float horizontalX = playerWorldPosition.x - observerWorldPosition.x;
        float horizontalZ = playerWorldPosition.z - observerWorldPosition.z;
        if (horizontalX * horizontalX + horizontalZ * horizontalZ <= MINIMUM_HORIZONTAL_DISTANCE_SQUARED)
        {
            return false;
        }

        float heading = Mathf.Repeat(compassHeadingDegrees, 360f);
        float targetBearing = Mathf.Repeat(Mathf.Atan2(horizontalX, horizontalZ) * Mathf.Rad2Deg, 360f);
        float signedAngle = Mathf.DeltaAngle(heading, targetBearing);

        RemotePlayerCompassMarkerKind markerKind;
        float displayBearing;
        if (Mathf.Abs(signedAngle) < visibleHalfAngleDegrees)
        {
            markerKind = RemotePlayerCompassMarkerKind.Blip;
            displayBearing = targetBearing;
        }
        else if (signedAngle < 0f)
        {
            markerKind = RemotePlayerCompassMarkerKind.LeftArrow;
            displayBearing = Mathf.Repeat(heading - visibleHalfAngleDegrees, 360f);
        }
        else
        {
            // Mathf.DeltaAngle resolves the exactly-behind tie to +180, so it deterministically points right.
            markerKind = RemotePlayerCompassMarkerKind.RightArrow;
            displayBearing = Mathf.Repeat(heading + visibleHalfAngleDegrees, 360f);
        }

        projection = new RemotePlayerCompassProjection(sessionId, markerKind, targetBearing, displayBearing, signedAngle);
        return true;
    }

    internal static bool TryGetVisibleHalfAngle(float compassAlphaFrom, float compassAlphaTo, out float visibleHalfAngleDegrees)
    {
        visibleHalfAngleDegrees = 0f;
        if (!IsFinite(compassAlphaFrom) || !IsFinite(compassAlphaTo))
        {
            return false;
        }

        float visibleFraction = compassAlphaTo - compassAlphaFrom;
        if (visibleFraction <= 0f || visibleFraction >= 1f)
        {
            return false;
        }

        visibleHalfAngleDegrees = visibleFraction * 180f;
        return visibleHalfAngleDegrees > 0f && visibleHalfAngleDegrees < 180f;
    }

    private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
