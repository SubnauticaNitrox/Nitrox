using System.Collections;
using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatInputField : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        private PlayerChatManager playerChatManager;
        private bool selected;
        private static float timeLeftUntilAutoClose;
        public static bool FreezeTime;
        public InputField InputField;

        private void Awake()
        {
            playerChatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
        }

        public void OnSelect(BaseEventData eventData)
        {
            playerChatManager.SelectChat();
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

        private void Update()
        {
            if (FreezeTime)
            {
                return;
            }

            if (selected)
            {
                if (UnityEngine.Input.GetKey(KeyCode.Return))
                {
                    if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
                    {
                        if (!InputField.text.EndsWith("\n"))
                        {
                            InputField.ActivateInputField();
                            InputField.text += "\n";
                            StartCoroutine(MoveToEndOfText());
                        }
                    }
                    else
                    {
                        playerChatManager.SendMessage();
                    }
                }
            }
            else
            {
                timeLeftUntilAutoClose -= Time.unscaledDeltaTime;
                if (timeLeftUntilAutoClose <= 0)
                {
                    playerChatManager.HideChat();
                    FreezeTime = true;
                }
            }
        }

        private IEnumerator MoveToEndOfText()
        {
            yield return null;
            InputField.MoveTextEnd(false);
        }
    }
}
