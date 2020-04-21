using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatPinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static PlayerChatManager playerChatManager;

        private readonly Camera mainCamera = Camera.main;
        private Vector2 screenRes = new Vector2(1920f, 1200f);
        private Vector2 chatSize;
        private Vector4 screenBorder;
        private Vector2 offset;
        private bool drag;

        private void Awake()
        {
            playerChatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
            chatSize = transform.parent.parent.GetComponent<RectTransform>().sizeDelta;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            screenRes.y = (screenRes.x / Screen.width) * Screen.height;
            offset = GetMouseWorldPosition() - (Vector2)playerChatManager.PlayerChaTransform.localPosition;
            screenBorder = new Vector4(-(screenRes.x - chatSize.x) / 2f, (screenRes.x - chatSize.x) / 2f, -(screenRes.y - chatSize.y) / 2f, (screenRes.y - chatSize.y) / 2f);

            drag = true;
            PlayerChatInputField.FreezeTime = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            drag = false;
            PlayerChatInputField.FreezeTime = false;
            PlayerChatInputField.ResetTimer();
        }

        private void Update()
        {
            if (drag)
            {
                playerChatManager.PlayerChaTransform.localPosition = GetChatPosition();
            }
        }

        private Vector2 GetMouseWorldPosition()
        {
            Vector3 position = mainCamera.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
            position.x = (position.x - 0.5f) * screenRes.x;
            position.y = (position.y - 0.5f) * screenRes.y;
            return position;
        }

        private Vector2 GetChatPosition()
        {
            Vector2 position = GetMouseWorldPosition() - offset;
            position.x = Mathf.Clamp(position.x, screenBorder.x, screenBorder.y);
            position.y = Mathf.Clamp(position.y, screenBorder.z, screenBorder.w);
            return position;
        }
    }
}
