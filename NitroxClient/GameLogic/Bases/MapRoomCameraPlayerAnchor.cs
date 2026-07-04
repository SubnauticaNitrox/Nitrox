using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public static class MapRoomCameraPlayerAnchor
{
    private static bool active;
    private static Vector3 position;
    private static Vector3 velocity;
    private static Quaternion bodyRotation;
    private static Quaternion aimingRotation;

    public static bool Active => active;

    public static void Begin(Vector3 currentPosition, Quaternion currentBodyRotation, Quaternion currentAimingRotation)
    {
        if (active)
        {
            return;
        }

        active = true;
        position = currentPosition;
        velocity = Vector3.zero;
        bodyRotation = currentBodyRotation;
        aimingRotation = currentAimingRotation;
    }

    public static void End()
    {
        if (!active)
        {
            return;
        }

        active = false;
    }

    public static bool TryGet(out Vector3 anchoredPosition, out Vector3 anchoredVelocity, out Quaternion anchoredBodyRotation, out Quaternion anchoredAimingRotation)
    {
        anchoredPosition = position;
        anchoredVelocity = velocity;
        anchoredBodyRotation = bodyRotation;
        anchoredAimingRotation = aimingRotation;

        return active;
    }
}
