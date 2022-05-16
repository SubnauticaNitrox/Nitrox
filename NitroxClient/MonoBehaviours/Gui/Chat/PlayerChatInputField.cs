using System.Collections;
using System.Collections.Generic;
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

        // Chat history
        private const int historyLength = 32; // 2^5 messages availables :D

        private List<string> sentMessages;
        private int _sentMessagesIndex;
        private int sentMessagesIndex
        {
            get { return _sentMessagesIndex; }
            set
            {
                if (sentMessages.Count == 0)
                {
                    // -1 is the state when there's no message sent
                    _sentMessagesIndex = -1;
                }
                else if (value < 1)
                {
                    sentMessagesIndex = 1;
                }
                else if (value > sentMessages.Count)
                {
                    _sentMessagesIndex = sentMessages.Count;
                }
                else
                {
                    // normal functionning
                    InputField.text = sentMessages[value - 1];
                    _sentMessagesIndex = value;
                }
            }
        }

        private void Awake()
        {
            playerChatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
            sentMessages = new();
            sentMessagesIndex = -1;
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
                if (!string.IsNullOrEmpty(InputField.text))
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
                            // Detect if there's a ghost message on top of the list (one that wasn't sent but still saved)
                            if (sentMessagesIndex != sentMessages.Count && sentMessages.Count > 0)
                            {
                                sentMessages.RemoveAt(sentMessages.Count - 1);
                            }

                            // If the list is too long, we'll just remove the first message of the list
                            if (sentMessages.Count > historyLength)
                            {
                                sentMessages.RemoveAt(0);
                            }

                            sentMessages.Add(InputField.text);
                            _sentMessagesIndex = sentMessages.Count;
                            playerChatManager.SendMessage();
                        }
                    }
                }
                
                // Chat history stuff
                // GetKeyDown means it's only getting executed once per press
                if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // If we're currently on the newest message, we want to save it to be able to come back to it (a ghost message)
                    if (sentMessagesIndex == sentMessages.Count && sentMessages.Count > 0)
                    {
                        sentMessages.Add(InputField.text);
                        _sentMessagesIndex = sentMessages.Count;
                    }
                    sentMessagesIndex--;
                }
                else if(UnityEngine.Input.GetKeyDown(KeyCode.DownArrow))
                {
                    // We shouldn't execute this check if we're already on top of the list
                    if (sentMessagesIndex < sentMessages.Count)
                    {
                        sentMessagesIndex++;
                        // If we're back to the newest message, we can delete it from the list because it has not been sent yet
                        if (sentMessagesIndex == sentMessages.Count && sentMessages.Count > 0)
                        {
                            sentMessages.RemoveAt(sentMessages.Count - 1);
                            _sentMessagesIndex = sentMessages.Count;
                        }
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
