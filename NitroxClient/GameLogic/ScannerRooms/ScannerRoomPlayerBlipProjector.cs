using System;
using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using UnityEngine;

namespace NitroxClient.GameLogic.ScannerRooms;

internal readonly record struct ScannerRoomPlayerLocation(SessionId SessionId, string PlayerName, Vector3 WorldPosition);

internal readonly record struct ScannerRoomPlayerBlip(SessionId SessionId, string PlayerName, Vector3 LocalPosition);

/// <summary>
/// Projects synchronized remote-player positions into a Scanner Room hologram.
/// This contains no Unity object lifecycle work so range and reconciliation behavior stay deterministic and testable.
/// </summary>
internal static class ScannerRoomPlayerBlipProjector
{
    public static IReadOnlyList<ScannerRoomPlayerBlip> Project(
        IEnumerable<ScannerRoomPlayerLocation> players,
        Vector3 scannerOrigin,
        Vector3 mapOrigin,
        float scanRange,
        float mapScale)
    {
        if (players == null)
        {
            throw new ArgumentNullException(nameof(players));
        }

        if (!IsFinite(scannerOrigin) ||
            !IsFinite(mapOrigin) ||
            !IsFinite(scanRange) ||
            !IsFinite(mapScale) ||
            scanRange < 0f)
        {
            return [];
        }

        float scanRangeSquared = scanRange * scanRange;
        Dictionary<SessionId, ScannerRoomPlayerBlip> projectedPlayers = [];

        foreach (ScannerRoomPlayerLocation player in players)
        {
            if (!IsFinite(player.WorldPosition) ||
                (player.WorldPosition - scannerOrigin).sqrMagnitude > scanRangeSquared)
            {
                continue;
            }

            Vector3 localPosition = (player.WorldPosition - mapOrigin) * mapScale;
            projectedPlayers[player.SessionId] = new ScannerRoomPlayerBlip(player.SessionId, player.PlayerName, localPosition);
        }

        return projectedPlayers.Values.OrderBy(player => player.SessionId).ToList();
    }

    private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
}
