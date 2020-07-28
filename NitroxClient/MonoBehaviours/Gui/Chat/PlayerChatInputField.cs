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
        public static bool FreezeTime { get; set; }
        public InputField InputField { get; set; }

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
            if (selected && InputField.text != "")
            {
                ResetTimer();
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
            else if (!FreezeTime)
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
