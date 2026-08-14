using System;
using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.GameLogic.HUD;

internal readonly record struct RemotePlayerCompassLayoutInput(
    SessionId SessionId,
    RemotePlayerCompassMarkerKind MarkerKind,
    Vector2 BasePosition);

internal readonly record struct RemotePlayerCompassLayoutResult(
    SessionId SessionId,
    RemotePlayerCompassMarkerKind MarkerKind,
    Vector2 Position);

internal static class RemotePlayerCompassMarkerLayout
{
    internal const float CollisionWidth = 12f;
    internal const float StackSpacing = 10f;

    public static IReadOnlyList<RemotePlayerCompassLayoutResult> Arrange(IEnumerable<RemotePlayerCompassLayoutInput> markers)
    {
        if (markers == null)
        {
            throw new ArgumentNullException(nameof(markers));
        }

        List<RemotePlayerCompassLayoutInput> validMarkers = markers
                                                                  .Where(marker => IsFinite(marker.BasePosition))
                                                                  .OrderBy(marker => marker.SessionId)
                                                                  .ToList();
        List<List<RemotePlayerCompassLayoutInput>> groups = [];

        AddArrowGroup(validMarkers, RemotePlayerCompassMarkerKind.LeftArrow, groups);
        AddArrowGroup(validMarkers, RemotePlayerCompassMarkerKind.RightArrow, groups);
        AddBlipGroups(validMarkers, groups);

        List<RemotePlayerCompassLayoutResult> results = [];
        foreach (List<RemotePlayerCompassLayoutInput> group in groups)
        {
            List<RemotePlayerCompassLayoutInput> orderedGroup = group.OrderBy(marker => marker.SessionId).ToList();
            float centerIndex = (orderedGroup.Count - 1) * 0.5f;
            float groupCenterY = orderedGroup.Average(marker => marker.BasePosition.y);
            for (int index = 0; index < orderedGroup.Count; index++)
            {
                RemotePlayerCompassLayoutInput marker = orderedGroup[index];
                Vector2 position = new(marker.BasePosition.x, groupCenterY + (index - centerIndex) * StackSpacing);
                results.Add(new RemotePlayerCompassLayoutResult(marker.SessionId, marker.MarkerKind, position));
            }
        }

        return results.OrderBy(result => result.SessionId).ToList();
    }

    private static void AddArrowGroup(
        IEnumerable<RemotePlayerCompassLayoutInput> markers,
        RemotePlayerCompassMarkerKind markerKind,
        ICollection<List<RemotePlayerCompassLayoutInput>> groups)
    {
        List<RemotePlayerCompassLayoutInput> arrowGroup = markers.Where(marker => marker.MarkerKind == markerKind).ToList();
        if (arrowGroup.Count > 0)
        {
            groups.Add(arrowGroup);
        }
    }

    private static void AddBlipGroups(
        IEnumerable<RemotePlayerCompassLayoutInput> markers,
        ICollection<List<RemotePlayerCompassLayoutInput>> groups)
    {
        List<RemotePlayerCompassLayoutInput> blips = markers
                                                         .Where(marker => marker.MarkerKind == RemotePlayerCompassMarkerKind.Blip)
                                                         .OrderBy(marker => marker.BasePosition.x)
                                                         .ThenBy(marker => marker.SessionId)
                                                         .ToList();
        List<RemotePlayerCompassLayoutInput>? currentGroup = null;
        float previousX = 0f;
        foreach (RemotePlayerCompassLayoutInput blip in blips)
        {
            if (currentGroup == null || blip.BasePosition.x - previousX > CollisionWidth)
            {
                currentGroup = [];
                groups.Add(currentGroup);
            }

            currentGroup.Add(blip);
            previousX = blip.BasePosition.x;
        }
    }

    private static bool IsFinite(Vector2 value) => IsFinite(value.x) && IsFinite(value.y);

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
