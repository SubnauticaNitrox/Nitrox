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

        public static Vector3 GetCorrectedVelocity(Vector3 remotePosition, Vector3 remoteVelocity, GameObject gameObject, float correctionTime)
        {
            Vector3 difference = (remotePosition - gameObject.transform.position);
            Vector3 velocityToMakeUpDifference = difference / correctionTime;

            float distance = difference.magnitude;

            if (distance > 20f)
            {
                // This should be a one-off teleport.
                gameObject.transform.position = remotePosition;
            }
            else
            {
                remoteVelocity = velocityToMakeUpDifference;
            }

            return remoteVelocity;
        }

        public static Vector3 GetCorrectedAngularVelocity(Quaternion remoteRotation, Vector3 angularVelocty, GameObject gameObject, float correctionTime)
        {
            Quaternion delta = remoteRotation * gameObject.transform.rotation.GetInverse();

            float angle;
            Vector3 axis;
            delta.ToAngleAxis(out angle, out axis);

            // We get an infinite axis in the event that our rotation is already aligned.
            if (float.IsInfinity(axis.x))
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
    }
}
