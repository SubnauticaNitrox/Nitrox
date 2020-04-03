using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatPinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static Transform chatTransform => PlayerChatManager.Main.PlayerChat.transform;
        private readonly Vector2 screenRes = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
        private bool drag;


        public void OnPointerDown(PointerEventData eventData)
        {
            drag = true;
            PlayerChatInputField.FreezeTime = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            drag = false;
            PlayerChatInputField.FreezeTime = false;
            PlayerChatInputField.ResetTimer();

            if (Mathf.Abs(chatTransform.localPosition.x * 2) >= screenRes.x || Mathf.Abs(chatTransform.localPosition.y * 2) >= screenRes.y)
            {
                chatTransform.localPosition = new Vector3(-500,125,0);
            }
        }

        private void Update()
        {
            if (drag)
            {
                Vector3 viewport = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
                chatTransform.localPosition = new Vector2((viewport.x - 0.5f) * screenRes.x, (viewport.y - 0.5f) * screenRes.y);
            }
        }
    }
}
