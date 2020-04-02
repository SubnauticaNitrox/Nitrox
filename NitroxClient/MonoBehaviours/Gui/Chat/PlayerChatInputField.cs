using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatInputField : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private bool selected;
        private float timeLeftUntilAutoClose;

        public void OnSelect(BaseEventData eventData)
        {
            PlayerChatManager.Main.PlayerChat.Select(true);
            selected = true;
            timeLeftUntilAutoClose = PlayerChat.CHAT_VISIBILITY_TIME_LENGTH;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
        }

        private void LateUpdate()
        {
            if (selected)
            {
                if (UnityEngine.Input.GetKey(KeyCode.Return))
                {
                    PlayerChatManager.Main.PlayerChat.SendChatMessage();
                }
            }
            else
            {
                timeLeftUntilAutoClose -= Time.unscaledDeltaTime;
                if (timeLeftUntilAutoClose <= 0)
                {
                    PlayerChatManager.Main.HideChat();
                }
            }
        }
    }
}
