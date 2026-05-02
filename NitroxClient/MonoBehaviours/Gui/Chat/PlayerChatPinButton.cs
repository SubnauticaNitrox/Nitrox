using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatPinButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private static PlayerChatManager playerChatManager;

        private readonly Camera mainCamera = Camera.main;
        private Vector2 chatSize;
        private Vector4 screenBorder;
        private Vector2 offset;
        private bool drag;
        private Canvas parentCanvas;

        private void Awake()
        {
            playerChatManager = PlayerChatManager.Instance;
            chatSize = transform.parent.parent.GetComponent<RectTransform>().sizeDelta;
            parentCanvas = GetComponentInParent<Canvas>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Use the canvas reference resolution scaled by actual screen aspect ratio
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            
            offset = GetMouseWorldPosition(canvasSize) - (Vector2)playerChatManager.PlayerChatTransform.localPosition;
            screenBorder = new Vector4(-(canvasSize.x - chatSize.x) / 2f, (canvasSize.x - chatSize.x) / 2f, -(canvasSize.y - chatSize.y) / 2f, (canvasSize.y - chatSize.y) / 2f);

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
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                playerChatManager.PlayerChatTransform.localPosition = GetChatPosition(canvasRect.sizeDelta);
            }
        }

        private Vector2 GetMouseWorldPosition(Vector2 canvasSize)
        {
            Vector3 position = mainCamera.ScreenToViewportPoint(UnityEngine.Input.mousePosition);
            position.x = (position.x - 0.5f) * canvasSize.x;
            position.y = (position.y - 0.5f) * canvasSize.y;
            return position;
        }

        private Vector2 GetChatPosition(Vector2 canvasSize)
        {
            Vector2 position = GetMouseWorldPosition(canvasSize) - offset;
            position.x = Mathf.Clamp(position.x, screenBorder.x, screenBorder.y);
            position.y = Mathf.Clamp(position.y, screenBorder.z, screenBorder.w);
            return position;
        }
    }
}
