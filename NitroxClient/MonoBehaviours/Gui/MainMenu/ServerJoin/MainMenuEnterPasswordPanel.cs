using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public class MainMenuEnterPasswordPanel : MonoBehaviour
{
    public const string NAME = "MultiplayerEnterPassword";

    private Rect serverPasswordWindowRect = new(Screen.width / 2 - 250, 200, 500, 200);

    public string serverPassword;
    public bool passwordEntered;
    public bool shouldFocus;
    public bool showingPasswordWindow;
    private IMultiplayerSession multiplayerSession;

    public void Setup(IMultiplayerSession _multiplayerSession)
    {
        multiplayerSession = _multiplayerSession;
    }

    private void OnGUI()
    {
        if (showingPasswordWindow)
        {
            serverPasswordWindowRect = GUILayout.Window(
                GUIUtility.GetControlID(FocusType.Keyboard),
                serverPasswordWindowRect,
                DoServerPasswordWindow,
                Language.main.Get("Nitrox_JoinServerPasswordHeader")
            );
        }
    }

    public void DoServerPasswordWindow(int windowId)
    {
        GUISkin GetGUISkin() => GUISkinUtils.RegisterDerivedOnce("menus.serverPassword",
                                                                 s =>
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
                                                                     s.label.fixedWidth = 80; //change this when adding new labels that need more space.

                                                                     s.button.fontSize = 14;
                                                                     s.button.stretchHeight = true;
                                                                 });

        Event e = Event.current;
        if (e.isKey)
        {
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    OnSubmitPasswordButtonClicked();
                    break;
                case KeyCode.Escape:
                    OnCancelButtonClicked();
                    break;
            }
        }

        GUISkinUtils.RenderWithSkin(GetGUISkin(),
                                    () =>
                                    {
                                        using (new GUILayout.VerticalScope("Box"))
                                        {
                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label(Language.main.Get("Nitrox_JoinServerPassword"));
                                                GUI.SetNextControlName("serverPasswordField");
                                                serverPassword = GUILayout.TextField(serverPassword);
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_SubmitPassword")))
                                            {
                                                HidePasswordWindow();
                                                OnSubmitPasswordButtonClicked();
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_Cancel")))
                                            {
                                                HidePasswordWindow();
                                                OnCancelButtonClicked();
                                                MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
                                            }
                                        }
                                    });

        if (shouldFocus)
        {
            GUI.FocusControl("serverPasswordField");
            shouldFocus = false;
        }
    }

    private void OnSubmitPasswordButtonClicked()
    {
        SubmitPassword();
        HidePasswordWindow();
    }

    private void SubmitPassword()
    {
        passwordEntered = true;
    }

    private void OnCancelButtonClicked()
    {
        multiplayerSession.Disconnect();
        HidePasswordWindow();
    }

    private void HidePasswordWindow()
    {
        showingPasswordWindow = false;
        shouldFocus = false;
    }
}
