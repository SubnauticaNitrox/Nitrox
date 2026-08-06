using System;
using UnityEngine;

namespace NitroxClient.GameLogic;

/// <summary>
/// Creates deterministic, non-piloting attachment points for Seamoth passengers.
/// The extra transform layer is important: <see cref="Vehicle.GetPilotingMode"/> only
/// treats a locked player parented directly to <see cref="Vehicle.playerPosition"/> as the pilot.
/// </summary>
public static class SeamothPassengerAnchors
{
    public const byte MaxPassengers = 3;

    private static readonly Vector3[] seatOffsets =
    [
        new(-0.32f, 0f, -0.18f),
        new(0.32f, 0f, -0.18f),
        new(0f, 0f, -0.48f)
    ];

    public static Transform GetOrCreate(SeaMoth seamoth, byte seatIndex)
    {
        if (!seamoth)
        {
            throw new ArgumentNullException(nameof(seamoth));
        }
        if (seatIndex >= MaxPassengers)
        {
            throw new ArgumentOutOfRangeException(nameof(seatIndex), seatIndex, "Invalid Seamoth passenger seat index");
        }

        Transform parent = seamoth.playerPosition.transform;
        string anchorName = GetAnchorName(seatIndex);
        Transform anchor = parent.Find(anchorName);
        if (anchor)
        {
            return anchor;
        }

        anchor = new GameObject(anchorName).transform;
        anchor.SetParent(parent, false);
        anchor.localPosition = seatOffsets[seatIndex];
        anchor.localRotation = Quaternion.identity;
        anchor.localScale = Vector3.one;
        return anchor;
    }

    public static void RemoveIfEmpty(Transform anchor)
    {
        if (anchor && anchor.childCount == 0)
        {
            UnityEngine.Object.Destroy(anchor.gameObject);
        }
    }

    private static string GetAnchorName(byte seatIndex) => $"NitroxSeamothPassengerSeat_{seatIndex}";
}
