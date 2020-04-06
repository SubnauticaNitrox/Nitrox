using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatInputField : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private bool selected;
        private static float timeLeftUntilAutoClose;
        public static bool FreezeTime;

        public void OnSelect(BaseEventData eventData)
        {
            PlayerChatManager.Main.SelectChat();
            selected = true;
            ResetTimer();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
        }

        public static void ResetTimer()
        {
            timeLeftUntilAutoClose = PlayerChat.CHAT_VISIBILITY_TIME_LENGTH;
            FreezeTime = false;
        }

        private void LateUpdate()
        {
            if (FreezeTime)
            {
                return;
            }

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
                    FreezeTime = true;
                }
            }
        }
    }
}
