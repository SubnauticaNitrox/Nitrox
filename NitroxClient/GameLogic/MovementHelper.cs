using UnityEngine;

namespace NitroxClient.GameLogic
{
    public static class MovementHelper
    {
        public static void MoveGameObject(GameObject go, Vector3 position, Quaternion rotation, float time)
        {
            iTween.MoveTo(go, iTween.Hash("position", position,
                                          "easetype", iTween.EaseType.easeInOutSine,
                                          "time", time));
            iTween.RotateTo(go, iTween.Hash("rotation", rotation.eulerAngles,
                                            "easetype", iTween.EaseType.easeInOutSine,
                                            "time", time));
        }
    }
}
