using System;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    //I'd like to see about skinning this behavior with the notification window that appears at the bottom of the screen during certain story events in the game.
    public class MainMenuNotification : MonoBehaviour
    {
        private bool awaitingAcknowledgement;
        private Action continuationAction;
        private string notificationMessage;
        private Rect notificationWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);

        public void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Return) ||
                UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                AcknowledgeNotification();
            }
        }

        public void OnGUI()
        {
            if (!string.IsNullOrEmpty(notificationMessage) && awaitingAcknowledgement)
            {
                notificationWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), notificationWindowRect, RenderUnableToJoinDialog, Language.main.Get("Nitrox_JoinServerFailed"));
            }
        }

        public void ShowNotification(string notificationMessage, Action continuationAction)
        {
            this.notificationMessage = notificationMessage;
            this.continuationAction = continuationAction;

            awaitingAcknowledgement = true;
        }

        private void RenderUnableToJoinDialog(int windowId)
        {
            GUISkinUtils.RenderWithSkin(GetGUISkin("dialogs.server.rejected", 550), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label(notificationMessage);
                    }

                    if (GUILayout.Button("OK"))
                    {
                        AcknowledgeNotification();
                    }
                }
            });
        }

        private void AcknowledgeNotification()
        {
            awaitingAcknowledgement = false;
            continuationAction?.Invoke();
        }

        private GUISkin GetGUISkin(string skinName, int labelWidth)
        {
            return GUISkinUtils.RegisterDerivedOnce(skinName, s =>
            {
                s.textField.fontSize = 20;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleCenter;
                s.textField.wordWrap = true;
                s.textField.stretchHeight = true;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 20;
                s.label.alignment = TextAnchor.MiddleCenter;
                s.label.stretchHeight = true;
                s.label.fixedWidth = labelWidth;

                s.button.fontSize = 16;
                s.button.stretchHeight = true;
            });
        }
    }
}
