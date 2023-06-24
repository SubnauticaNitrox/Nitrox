using System.Collections;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using UnityEngine;
using UWE;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public static class MainMenuAddServerWindow
{
    private static Rect addServerWindowRect = new(Screen.width / 2 - 250, 200, 500, 200);

    private static string serverHostInput;
    private static string serverNameInput;
    private static string serverPortInput;

    private static bool shouldFocus;
    private static bool showingAddServer;

    public static void OnExternalGUI()
    {
        if (showingAddServer)
        {
            addServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), addServerWindowRect, DoAddServerWindow, Language.main.Get("Nitrox_AddServer"));
        }
    }

    public static void ShowAddServerWindow()
    {
        serverNameInput = "local";
        serverHostInput = "127.0.0.1";
        serverPortInput = ServerList.DEFAULT_PORT.ToString();
        showingAddServer = true;
        shouldFocus = true;
        uGUI_MainMenu.main.canvasGroup.interactable = false;
    }

    public static void HideAddServerWindow()
    {
        IEnumerator SetWindowComponents()
        {
            showingAddServer = false;
            shouldFocus = true;
            yield return null;
            uGUI_MainMenu.main.canvasGroup.interactable = true;
        }

        CoroutineHost.StartCoroutine(SetWindowComponents());
    }

    private static void OnAddServerButtonClicked()
    {
        serverNameInput = serverNameInput.Trim();
        serverHostInput = serverHostInput.Trim();
        if (int.TryParse(serverPortInput.Trim(), out int serverPort))
        {
            MainMenuMultiplayerPanel.Main.CreateServerButton(serverNameInput, serverHostInput, serverPort);
            ServerList.Instance.Add(new ServerList.Entry(serverNameInput, serverHostInput, serverPort));
            ServerList.Instance.Save();

            HideAddServerWindow();
        }
        else
        {
            Log.InGame("Server port was not a valid number!");
        }
    }

    private static void OnCancelButtonClicked()
    {
        HideAddServerWindow();
    }

    private static void DoAddServerWindow(int windowId)
    {
        Event e = Event.current;
        if (e.isKey)
        {
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    OnAddServerButtonClicked();
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
                                                GUILayout.Label(Language.main.Get("Nitrox_AddServerName"));
                                                GUI.SetNextControlName("serverNameField");
                                                // 120 so users can't go too crazy.
                                                serverNameInput = GUILayout.TextField(serverNameInput, 120);
                                            }

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label(Language.main.Get("Nitrox_AddServerHost"));
                                                GUI.SetNextControlName("serverHostField");
                                                // 120 so users can't go too crazy.
                                                serverHostInput = GUILayout.TextField(serverHostInput, 120);
                                            }

                                            using (new GUILayout.HorizontalScope())
                                            {
                                                GUILayout.Label(Language.main.Get("Nitrox_AddServerPort"));
                                                GUI.SetNextControlName("serverPortField");
                                                serverPortInput = GUILayout.TextField(serverPortInput);
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_AddServerAdd")))
                                            {
                                                OnAddServerButtonClicked();
                                            }

                                            if (GUILayout.Button(Language.main.Get("Nitrox_Cancel")))
                                            {
                                                OnCancelButtonClicked();
                                            }
                                        }
                                    });

        if (shouldFocus)
        {
            GUI.FocusControl("serverNameField");
            shouldFocus = false;
        }
    }

    private static GUISkin GetGUISkin()
    {
        return GUISkinUtils.RegisterDerivedOnce("menus.server",
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
    }
}
