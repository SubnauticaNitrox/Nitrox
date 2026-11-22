using UnityEngine;

namespace NitroxClient.GameLogic
{
    public static class MovementHelper
    {
        public static void MoveRotateGameObject(GameObject go, Vector3 position, Quaternion rotation, float time)
        {
            MoveGameObject(go, position, time);
            RotateGameObject(go, rotation, time);
        }

        public static void MoveGameObject(GameObject go, Vector3 position, float time)
        {
            iTween.MoveTo(go, iTween.Hash("position", position,
                                          "easetype", iTween.EaseType.easeInOutSine,
                                          "time", time));
        }

        public static void RotateGameObject(GameObject go, Quaternion rotation, float time)
        {
            iTween.RotateTo(go, iTween.Hash("rotation", rotation.eulerAngles,
                                            "easetype", iTween.EaseType.easeInOutSine,
                                            "time", time));
        }

        public static void Stop(GameObject go)
        {
            iTween.Stop(go);
        }

        public static Vector3 GetCorrectedVelocity(Vector3 remotePosition, Vector3 remoteVelocity, Vector3 currentPosition, float correctionTime)
        {
            Vector3 difference = remotePosition - currentPosition;

            if (float.IsNaN(difference.x) || float.IsNaN(difference.y) || float.IsNaN(difference.z) || correctionTime == 0f)
            {
                return Vector3.zero;
            }

            return difference / correctionTime;
        }

        public static Vector3 GetCorrectedAngularVelocity(Quaternion remoteRotation, Vector3 angularVelocty, Quaternion currentRotation, float correctionTime)
        {
            Quaternion delta = remoteRotation * currentRotation.GetInverse();

            delta.ToAngleAxis(out float angle, out Vector3 axis);

            // We get an infinite axis in the event that our rotation is already aligned.
            if (float.IsInfinity(axis.x))
            {
                return angularVelocty;
            }

            // Guard for NaN when remoteRotation and gameobjects rotation are parallel (witnessed during macOS habitat transition)
            if (float.IsNaN(angle) || float.IsNaN(axis.x) || float.IsNaN(axis.y) || float.IsNaN(axis.z))
            {
                return angularVelocty;
            }

            if (angle > 180f)
            {
                angle -= 360f;
            }

            // Here I drop down to 0.9f times the desired movement,
            // since we'd rather undershoot and ease into the correct angle
            // than overshoot and oscillate around it in the event of errors.
            return (.9f * Mathf.Deg2Rad * angle / correctionTime) * axis + angularVelocty;
        }

        public static bool TeleportIfTooFar(Transform transform, Rigidbody rigidbody, Vector3 targetPosition, Quaternion targetRotation, float teleportThreshold)
        {
            if ((transform.position - targetPosition).sqrMagnitude <= teleportThreshold * teleportThreshold)
            {
                return false;
            }

            if (rigidbody)
            {
                rigidbody.position = targetPosition;
                rigidbody.rotation = targetRotation;
                rigidbody.velocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
            }
            else
            {
                transform.position = targetPosition;
                transform.rotation = targetRotation;
            }
            return true;
        }

        public static float GetTeleportThreshold(float velocity)
        {
            return Mathf.Max(5f, velocity * 1.5f);
        }
    }
}
