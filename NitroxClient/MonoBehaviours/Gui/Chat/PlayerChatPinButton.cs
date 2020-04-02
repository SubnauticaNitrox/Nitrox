using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatPinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static Transform chatTransform => PlayerChatManager.Main.PlayerChat.transform;
        private bool drag;
        private Vector3 offset;
        private Vector3 offset2;
        private Vector3 startPos;

        public void OnPointerDown(PointerEventData eventData)
        {
            startPos = chatTransform.position;
            offset = chatTransform.localPosition - UnityEngine.Input.mousePosition;
            offset2 = chatTransform.localPosition - Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            drag = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            drag = false;
        }

        private void Update()
        {
            if (drag)
            {
                Log.Debug(chatTransform.position);
                Log.Debug(UnityEngine.Input.mousePosition);
                Log.Debug(offset);
                Log.Debug(offset2);
                Log.Debug(Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition));
                if (chatTransform.position == startPos)
                {
                    chatTransform.position += new Vector3(100,100,0);
                }
                //chatTransform.position = (Vector2)(Camera.main.ScreenToWorldPoint(UnityEngine.Input.mousePosition) + offset);
                Log.Debug("=>" +chatTransform.position);
                Log.Debug("      ");
                //chatTransform.localPosition += new Vector3(1,1,0);
            }
        }
    }
}
