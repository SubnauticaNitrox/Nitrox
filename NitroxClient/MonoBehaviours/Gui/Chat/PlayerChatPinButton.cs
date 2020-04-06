using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatPinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static PlayerChatManager playerChatManager;

        private Vector2 screenRes = new Vector2(1920, 1200);
        private bool drag;

        private void Awake()
        {
            playerChatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            drag = true;
            screenRes.y = (screenRes.x / Screen.width) * Screen.height;
            PlayerChatInputField.FreezeTime = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            drag = false;
            PlayerChatInputField.FreezeTime = false;
            PlayerChatInputField.ResetTimer();

            if (Mathf.Abs(playerChatManager.PlayerChaTransform.localPosition.x * 2) >= screenRes.x || Mathf.Abs(playerChatManager.PlayerChaTransform.localPosition.y * 2) >= screenRes.y)
            {
                playerChatManager.PlayerChaTransform.localPosition = new Vector3(-500, 125, 0);
            }
        }

        private void FixedUpdate()
        {
            if (drag)
            {
                Vector3 viewport = Camera.main.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
                playerChatManager.PlayerChaTransform.localPosition = new Vector2((viewport.x - 0.5f) * screenRes.x, (viewport.y - 0.5f) * screenRes.y);
            }
        }
    }
}
