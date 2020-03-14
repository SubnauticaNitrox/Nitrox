using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using UnityEngine;

namespace NitroxClient.Debuggers
{
    public class EntityDebugger : BaseDebugger
    {
        public EntityDebugger() : base(200, null, KeyCode.E, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            ActiveTab = AddTab("EntityDebugger", RenderEntityDebugger);
        }

        private void RenderEntityDebugger()
        {
        }

        public override void OnGUIExtra()
        {
            foreach (KeyValuePair<NitroxId, GameObject> gameObjectPairs in NitroxEntity.GetGameObjects())
            {
                NitroxId id = gameObjectPairs.Key;
                GameObject gameObject = gameObjectPairs.Value;
                if (gameObject != null && gameObject != Player.mainObject)
                {
                    Vector3 screenPos = Player.main.viewModelCamera.WorldToScreenPoint(gameObject.transform.position);
                    if (screenPos.z > 0 && screenPos.z < 20 &&
                        screenPos.x >= 0 && screenPos.x < Screen.width &&
                        screenPos.y >= 0 && screenPos.y < Screen.height)
                    {
                        GUIStyle style = GUI.skin.label;
                        GUIContent textContent = new GUIContent("ID " + id.ToString());
                        Vector2 size = style.CalcSize(textContent);
                        GUI.Label(new Rect(screenPos.x, Screen.height - screenPos.y, size.x, size.y), textContent);
                    }
                }
            }
        }
    }
}
