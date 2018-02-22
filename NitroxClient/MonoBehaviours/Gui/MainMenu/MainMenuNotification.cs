using System;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
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
                notificationWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), notificationWindowRect, RenderUnableToJoinDialog, "Unable to Join Session");
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
            GUISkinUtils.RenderWithSkin(GetGUISkin("dialogs.server.rejected", 500), () =>
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
                s.textField.fontSize = 14;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleLeft;
                s.textField.wordWrap = true;
                s.textField.stretchHeight = true;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 14;
                s.label.alignment = TextAnchor.MiddleRight;
                s.label.stretchHeight = true;
                s.label.fixedWidth = labelWidth;

                s.button.fontSize = 14;
                s.button.stretchHeight = true;
            });
        }
    }
}
