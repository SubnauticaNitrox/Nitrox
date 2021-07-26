using NitroxClient.GameLogic.ChatUI;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Chat
{
    public class PlayerChatLogItem : MonoBehaviour
    {
        private Text playerName;
        private Text time;
        private Text message;

        private void SetupComponents()
        {
            Text[] textFields = gameObject.GetComponentsInChildren<Text>();
            playerName = textFields[0];
            time = textFields[1];
            message = textFields[2];
        }

        public void ApplyOnPrefab(ChatLogEntry chatLogEntry)
        {
            if (playerName == null)
            {
                SetupComponents();
            }

            playerName.text = chatLogEntry.PlayerName;
            playerName.color = chatLogEntry.PlayerColor;
            time.text = chatLogEntry.Time;
            message.text = chatLogEntry.MessageText;
            gameObject.SetActive(true);
        }
    }
}
