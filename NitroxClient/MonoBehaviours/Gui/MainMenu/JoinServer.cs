using System;
using System.Collections;
using System.Diagnostics;
using System.Text.RegularExpressions;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        private GameObject colorPickerPanelPrototype;
        private GameObject saveGameMenuPrototype;
        private MainMenuRightSide rightSideMainMenu;

        private Rect serverPasswordWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 200);
        private PlayerPreferenceManager preferencesManager;
        private PlayerPreference activePlayerPreference;
        private IMultiplayerSession multiplayerSession;

        private GameObject joinServerMenu;
        private GameObject multiplayerClient;
        private GameObject playerSettingsPanel;
        private GameObject lowerDetailTextGameObject;
        private uGUI_InputField playerNameInputField;
        private uGUI_ColorPicker colorPicker;
        private RectTransform joinServerBackground;

        private string serverIp;
        private int serverPort;

        private bool isSubscribed;
        private bool shouldFocus;
        private bool showingPasswordWindow;
        private bool passwordEntered;
        private string serverPassword = string.Empty;

        public string MenuName => joinServerMenu.AliveOrNull()?.name ?? throw new Exception("Menu not yet initialized");

        public void Setup(GameObject saveGameMenu)
        {
            saveGameMenuPrototype = saveGameMenu;
            InitializeJoinMenu();

            DontDestroyOnLoad(gameObject);
            Hide();
        }

        public void Show(string ip, int port)
        {
            NitroxServiceLocator.BeginNewLifetimeScope();
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            preferencesManager = NitroxServiceLocator.LocateService<PlayerPreferenceManager>();

            gameObject.SetActive(true);
            serverIp = ip;
            serverPort = port;

            //Set Server IP in info label
            lowerDetailTextGameObject.GetComponent<Text>().text = $"{Language.main.Get("Nitrox_JoinServerIpAddress")}\n{(NitroxPrefs.HideIp.Value ? "****" : serverIp)}";

            //Initialize elements from preferences
            activePlayerPreference = preferencesManager.GetPreference(serverIp);
            SubscribeColorChanged();

            // HSV => Hue Saturation Value, HSB => Hue Saturation Brightness
            Color.RGBToHSV(activePlayerPreference.PreferredColor(), out float hue, out _, out float brightness);
            colorPicker.SetHSB(new Vector3(hue, 1f, brightness));

            playerNameInputField.text = activePlayerPreference.PlayerName;

            StartMultiplayerClient();
        }

        private void Hide()
        {
            UnsubscribeColorChanged();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (multiplayerSession.CurrentState.CurrentStage != MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS ||
                gameObject.GetComponent<MainMenuNotification>() != null)
            {
                return;
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
            {
                OnJoinClick();
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                OnCancelClick();
            }
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

        private void OnDestroy()
        {
            UnsubscribeColorChanged();
        }

        private void SubscribeColorChanged()
        {
            if (isSubscribed)
            {
                return;
            }

            colorPicker.onColorChange.AddListener(OnColorChange);

            isSubscribed = true;
        }

        private void UnsubscribeColorChanged()
        {
            if (!playerSettingsPanel || !isSubscribed)
            {
                return;
            }

            colorPicker.onColorChange.RemoveListener(OnColorChange);

            isSubscribed = false;
        }

        private void OnColorChange(ColorChangeEventData eventData)
        {
            Color selectedColor = eventData.color;

            GameObject selectedColorGameObject = playerSettingsPanel.RequireGameObject("BaseTab/SelectedColor");

            Image baseTabSelectedColorImage = selectedColorGameObject.GetComponent<Image>();
            baseTabSelectedColorImage.color = selectedColor;
        }

        private void FocusPlayerNameTextbox()
        {
            playerNameInputField.ActivateInputField();
        }

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject("Multiplayer Client");
                multiplayerClient.AddComponent<Multiplayer>();
                multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
            }

            try
            {
                multiplayerSession.Connect(serverIp, serverPort);
            }
            catch (ClientConnectionFailedException)
            {
                Log.InGameSensitive(Language.main.Get("Nitrox_UnableToConnect") + " {ip}:{port}", serverIp, serverPort);

                if (serverIp.Equals("127.0.0.1"))
                {
                    if (Process.GetProcessesByName("NitroxServer-Subnautica").Length == 0)
                    {
                        Log.InGame(Language.main.Get("Nitrox_StartServer"));
                    }
                    else
                    {
                        Log.InGame(Language.main.Get("Nitrox_FirewallInterfering"));
                    }
                }
                OnCancelClick();
            }
        }

        private void OnCancelClick()
        {
            StopMultiplayerClient();
            rightSideMainMenu.OpenGroup("Multiplayer");
            Hide();
        }

        private void OnJoinClick()
        {
            string playerName = playerNameInputField.text;

            //https://regex101.com/r/eTWiEs/2/
            if (!Regex.IsMatch(playerName, @"^[a-zA-Z0-9._-]{3,25}$"))
            {
                NotifyUser(Language.main.Get("Nitrox_InvalidUserName"));
                return;
            }
            preferencesManager.SetPreference(serverIp, new PlayerPreference(playerName, colorPicker.currentColor));

            AuthenticationContext authenticationContext = passwordEntered ? new AuthenticationContext(playerName, serverPassword) : new AuthenticationContext(playerName);

            multiplayerSession.RequestSessionReservation(new PlayerSettings(colorPicker.currentColor.ToDto()), authenticationContext);
        }

        private void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY:
                    Log.InGame(Language.main.Get("Nitrox_RequestingSessionPolicy"));
                    break;

                case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:
                    if (multiplayerSession.SessionPolicy.RequiresServerPassword)
                    {
                        Log.InGame(Language.main.Get("Nitrox_WaitingPassword"));
                        showingPasswordWindow = true;
                        shouldFocus = true;
                    }
                    Log.InGame(Language.main.Get("Nitrox_WaitingUserInput"));
                    rightSideMainMenu.OpenGroup("Join Server");
                    FocusPlayerNameTextbox();
                    break;

                case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                    Log.InGame(Language.main.Get("Nitrox_LaunchGame"));
                    multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                    preferencesManager.Save();

#pragma warning disable CS0618 // God Damn it UWE...
                    IEnumerator startNewGame = uGUI_MainMenu.main.StartNewGame(GameMode.Survival);
#pragma warning restore CS0618 // God damn it UWE...
                    StartCoroutine(startNewGame);
                    LoadingScreenVersionText.Initialize();

                    break;

                case MultiplayerSessionConnectionStage.SESSION_RESERVATION_REJECTED:
                    Log.InGame(Language.main.Get("Nitrox_RejectedSessionPolicy"));

                    MultiplayerSessionReservationState reservationState = multiplayerSession.Reservation.ReservationState;

                    string reservationRejectionNotification = reservationState.Describe();

                    NotifyUser(
                        reservationRejectionNotification,
                        () =>
                        {
                            multiplayerSession.Disconnect();
                            multiplayerSession.Connect(serverIp, serverPort);
                        });
                    break;

                case MultiplayerSessionConnectionStage.DISCONNECTED:
                    Log.Info(Language.main.Get("Nitrox_DisconnectedSession"));
                    break;
            }
        }

        private void NotifyUser(string notificationMessage, Action continuationAction = null)
        {
            if (gameObject.GetComponent<MainMenuNotification>() != null)
            {
                return;
            }

            MainMenuNotification notificationDialog = gameObject.AddComponent<MainMenuNotification>();
            notificationDialog.ShowNotification(notificationMessage, () =>
            {
                continuationAction?.Invoke();
                Destroy(gameObject.GetComponent<MainMenuNotification>(), 0.0001f);
            });
        }

        public void StopMultiplayerClient()
        {
            if (!multiplayerClient)
            {
                return;
            }

            Multiplayer.Main.StopCurrentSession();
            Destroy(multiplayerClient);
            multiplayerClient = null;
            if (multiplayerSession != null)
            {
                multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
            }
        }

        //This method merges the cloned color picker element with the existing template for menus that appear in the "right side" region of the main menu.
        private void InitializeJoinMenu()
        {
            colorPickerPanelPrototype = Resources.Load<GameObject>("WorldEntities/Tools/RocketBase").RequireGameObject("Base/BuildTerminal/GUIScreen/CustomizeScreen/Panel/");
            rightSideMainMenu = MainMenuRightSide.main;

            joinServerMenu = CloneSaveGameMenuPrototype();

            joinServerMenu.transform.SetParent(rightSideMainMenu.transform, false);
            rightSideMainMenu.groups.Add(joinServerMenu.GetComponent<MainMenuGroup>());

            //Not sure what is up with this menu, but we have to use the RectTransform of the Image component as the parent for our color picker panel.
            //Most of the UI elements seem to vanish behind this Image otherwise.
            joinServerBackground = joinServerMenu.GetComponent<Image>().rectTransform;
            joinServerBackground.anchorMin = new Vector2(0.5f, 0.5f);
            joinServerBackground.anchorMax = new Vector2(0.5f, 0.5f);
            joinServerBackground.pivot = new Vector2(0.5f, 0.5f);
            joinServerBackground.anchoredPosition = new Vector2(joinServerBackground.anchoredPosition.x, 5f);

            InitializePlayerSettingsPanel();
        }

        //This configures and re-positions the elements on the default "ColorGreyscale" menu to suite our purposes now.
        private void InitializePlayerSettingsPanel()
        {
            InstantiateColorPickerPanelPrototype();
            InitializePlayerSettingsPanelElement();
            InitializeBaseTabElement();
            InitializeLowerDetailElement();
            InitializePlayerNameInputElement();
            InitializeColorPickerComponent();
            InitializeColorPickerElement();
            InitializeButtonElements();
        }

        //Join and Cancel buttons
        private void InitializeButtonElements()
        {
            GameObject cancelButtonGameObject = playerSettingsPanel.RequireGameObject("Button");
            GameObject joinButtonGameObject = Instantiate(cancelButtonGameObject, playerSettingsPanel.transform, false);

            //Click events
            Button cancelButton = cancelButtonGameObject.GetComponent<Button>();
            cancelButton.onClick.AddListener(OnCancelClick);

            Button joinButton = joinButtonGameObject.GetComponent<Button>();
            joinButton.onClick.AddListener(OnJoinClick);

            RectTransform cancelButtonTransform = (RectTransform)cancelButtonGameObject.transform;
            GameObject cancelButtonTextGameObject = cancelButtonTransform.RequireGameObject("Text");
            cancelButtonTextGameObject.GetComponent<Text>().text = Language.main.Get("Nitrox_Cancel");

            cancelButtonTransform.sizeDelta = new Vector2(cancelButtonTransform.rect.width * 0.85f, cancelButtonTransform.rect.height);
            cancelButtonTransform.anchoredPosition = new Vector2(
                -1f * joinServerBackground.rect.width / 2f + cancelButtonTransform.rect.width / 2f,
                -1f * (joinServerBackground.rect.height / 2f) + cancelButtonTransform.rect.height / 2f + 3f);

            RectTransform joinButtonTransform = (RectTransform)joinButtonGameObject.transform;
            joinButtonTransform.anchoredPosition = new Vector2(
                joinServerBackground.rect.width / 2f - joinButtonTransform.rect.width / 2f + 20f,
                -1f * (joinServerBackground.rect.height / 2f) + joinButtonTransform.rect.height / 2f + 3f);

            //Flip the button over
            joinButtonTransform.sizeDelta = new Vector2(joinButtonTransform.rect.width * 0.85f, joinButtonTransform.rect.height);
            joinButtonTransform.Rotate(Vector3.forward * -180);

            GameObject joinButtonTextGameObject = joinButtonTransform.RequireGameObject("Text");
            joinButtonTextGameObject.GetComponent<Text>().text = Language.main.Get("Nitrox_Join");

            //Flip the text so it is no longer upside down after flipping the button.
            RectTransform joinButtonTextRectTransform = (RectTransform)joinButtonTextGameObject.transform;
            joinButtonTextRectTransform.Rotate(Vector3.forward * -180);
        }

        private GameObject CloneSaveGameMenuPrototype()
        {
            joinServerMenu = Instantiate(saveGameMenuPrototype);
            Destroy(joinServerMenu.RequireGameObject("Header"));
            Destroy(joinServerMenu.RequireGameObject("Scroll View"));
            Destroy(joinServerMenu.GetComponent<LayoutGroup>());
            Destroy(joinServerMenu.GetComponent<MainMenuLoadPanel>());
            joinServerMenu.GetAllComponentsInChildren<LayoutGroup>().ForEach(Destroy);

            //We cannot register click events on child transforms if they are being captured here.
            joinServerMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;
            joinServerMenu.name = "Join Server";

            return joinServerMenu;
        }

        private void InstantiateColorPickerPanelPrototype()
        {
            //Create a clone of the RocketBase color picker panel.
            playerSettingsPanel = Instantiate(colorPickerPanelPrototype);
            GameObject baseTab = playerSettingsPanel.RequireGameObject("BaseTab");
            GameObject serverNameLabel = playerSettingsPanel.RequireGameObject("Name Label");
            GameObject stripe1Tab = playerSettingsPanel.RequireGameObject("Stripe1Tab");
            GameObject stripe2Tab = playerSettingsPanel.RequireGameObject("Stripe2Tab");
            GameObject nameTab = playerSettingsPanel.RequireGameObject("NameTab");
            GameObject frontOverlay = playerSettingsPanel.RequireGameObject("FrontOverlay");
            GameObject colorLabel = playerSettingsPanel.RequireGameObject("Color Label");

            //Enables pointer events that are a required for the uGUI_ColorPicker to work.
            CanvasGroup colorPickerCanvasGroup = playerSettingsPanel.AddComponent<CanvasGroup>();
            colorPickerCanvasGroup.blocksRaycasts = true;
            colorPickerCanvasGroup.ignoreParentGroups = true;
            colorPickerCanvasGroup.interactable = true;

            //Destroy everything that we know we will not be using.
            Destroy(playerSettingsPanel.transform.parent);
            Destroy(playerSettingsPanel.GetComponent<uGUI_NavigableControlGrid>());
            Destroy(playerSettingsPanel.GetComponent<Image>());
            Destroy(baseTab.GetComponent<Button>());
            Destroy(stripe1Tab);
            Destroy(stripe2Tab);
            Destroy(nameTab);
            Destroy(colorLabel);
            Destroy(serverNameLabel);

            //We can't just destroy the game object for some reason. The image still hangs around.
            //Destruction of the actual overlay game object is done for good measure.
            Destroy(frontOverlay.GetComponent<Image>());
            Destroy(frontOverlay);
        }

        //This panel acts as the parent of all other UI elements on the menu. It is parented by the cloned "SaveGame" menu.
        private void InitializePlayerSettingsPanelElement()
        {
            playerSettingsPanel.SetActive(true);

            RectTransform playerSettingsPanelTransform = (RectTransform)playerSettingsPanel.transform;
            playerSettingsPanelTransform.SetParent(joinServerBackground, false);
            playerSettingsPanelTransform.anchorMin = new Vector2(0f, 0f);
            playerSettingsPanelTransform.anchorMax = new Vector2(1f, 1f);
            playerSettingsPanelTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        //The base tab is the outline surrounding the color picker, as well as teh "Player Color" label and associated "Selected Color" image.
        private void InitializeBaseTabElement()
        {
            GameObject baseTab = playerSettingsPanel.RequireGameObject("BaseTab");

            //Re-position the border
            RectTransform baseTabTransform = (RectTransform)baseTab.transform;
            baseTabTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
            baseTabTransform.anchorMin = new Vector2(0.5f, 0.5f);
            baseTabTransform.anchorMax = new Vector2(0.5f, 0.5f);
            baseTabTransform.pivot = new Vector2(0.5f, 0.5f);

            //Resize the border to match the new parent. Capture the original width so we can make adjustments to child controls later
            //because the existing elements all have some really weird anchor settings the prevent them from moving automatically.
            float originalBaseTabWidth = baseTabTransform.rect.width;
            baseTabTransform.sizeDelta = new Vector2(joinServerBackground.rect.width, baseTabTransform.rect.height);

            //Move the SelectedColor element over to the right enough to match the shrinkage of the base tab.
            GameObject baseTabSelectedColor = baseTabTransform.RequireGameObject("SelectedColor");
            Image baseTabSelectedColorImage = baseTabSelectedColor.GetComponent<Image>();
            baseTabSelectedColorImage.rectTransform.anchoredPosition = new Vector2(
                baseTabSelectedColorImage.rectTransform.anchoredPosition.x + (originalBaseTabWidth - baseTabTransform.rect.width) / 2,
                baseTabSelectedColorImage.rectTransform.anchoredPosition.y);

            //Place the "Player Color" label to the right of the SelectedColor image and shrink it to fit the new tab region.
            GameObject baseTabTextGameObject = baseTabTransform.RequireGameObject("Text");
            RectTransform baseTabTextTransform = (RectTransform)baseTabTextGameObject.transform;
            baseTabTextTransform.anchorMin = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.anchorMax = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.pivot = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.sizeDelta = new Vector2(80, 35);

            baseTabTextTransform.anchoredPosition = new Vector2(
                baseTabSelectedColorImage.rectTransform.anchoredPosition.x + baseTabTextTransform.rect.width / 2f + 22f,
                baseTabSelectedColorImage.rectTransform.anchoredPosition.y);

            baseTabTextGameObject.GetComponent<Text>().text = Language.main.Get("Nitrox_PlayerColor");

            //This resizes the actual Image that outlines all of the UI elements.
            GameObject baseTabBackgroundGameObject = baseTabTransform.RequireGameObject("Background");
            Image baseTabBackgroundImage = baseTabBackgroundGameObject.GetComponent<Image>();
            baseTabBackgroundImage.rectTransform.sizeDelta = new Vector2(joinServerBackground.rect.width, baseTabTransform.rect.height);

            //This removes the extra "tabs" from the base texture.
            Texture2D newBaseTabTexture = baseTabBackgroundImage.sprite.texture.Clone();
            TextureBlock textureBlock = new TextureBlock(3, 3, 160, (int)(baseTabBackgroundImage.sprite.textureRect.height - 71f));
            IColorSwapStrategy alphaChannelSwapper = new AlphaChannelSwapper(0f);
            HsvSwapper baseTabBackgroundSwapper = new HsvSwapper(alphaChannelSwapper);
            baseTabBackgroundSwapper.SetHueRange(185f, 215f);
            baseTabBackgroundSwapper.SetAlphaRange(0f, 175f);
            newBaseTabTexture.SwapTextureColors(baseTabBackgroundSwapper, textureBlock);
            baseTabBackgroundImage.sprite = Sprite.Create(newBaseTabTexture, new Rect(baseTabBackgroundImage.sprite.textureRect), new Vector2(0f, 0f));
        }

        //The LowerDetail is the region that displays the current Server IP and the graphic that appears beneath it.
        private void InitializeLowerDetailElement()
        {
            GameObject lowerDetail = playerSettingsPanel.RequireGameObject("LowerDetail");

            //We use this as a reference point for positioning the LowerDetail element.
            RectTransform baseTabTextTransform = (RectTransform)playerSettingsPanel.RequireTransform("BaseTab/Text");

            RectTransform lowerDetailRectTransform = (RectTransform)lowerDetail.transform;
            lowerDetailRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.pivot = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.anchoredPosition = new Vector2(baseTabTextTransform.anchoredPosition.x - 24f, baseTabTextTransform.anchoredPosition.y - 61.4f);

            //The text element is right-aligned by default and needs to be centered for our purposes
            lowerDetailTextGameObject = lowerDetailRectTransform.RequireGameObject("Text");
            Text lowerDetailText = lowerDetailTextGameObject.GetComponent<Text>();
            lowerDetailText.resizeTextForBestFit = true;
            lowerDetailText.alignment = TextAnchor.MiddleCenter;

            RectTransform lowerDetailTextRectTransform = (RectTransform)lowerDetailTextGameObject.transform;
            lowerDetailTextRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.pivot = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.anchoredPosition = new Vector2(0, 0);

            //Delete the pixels under the IP
            Destroy(lowerDetail.RequireGameObject("Pixels"));
        }

        //Player name text box
        private void InitializePlayerNameInputElement()
        {
            GameObject playerNameInputFieldGameObject = playerSettingsPanel.RequireGameObject("InputField");
            RectTransform inputFieldRectTransform = (RectTransform)playerNameInputFieldGameObject.transform;
            inputFieldRectTransform.anchoredPosition = new Vector2(inputFieldRectTransform.anchoredPosition.x, inputFieldRectTransform.anchoredPosition.y - 15f);

            playerNameInputField = playerNameInputFieldGameObject.GetComponent<uGUI_InputField>();
            playerNameInputField.selectionColor = Color.white;

            GameObject inputFieldPlaceholder = inputFieldRectTransform.RequireGameObject("Placeholder");
            inputFieldPlaceholder.GetComponent<Text>().text = Language.main.Get("Nitrox_EnterName");
        }

        //This is the "service" that manages the click and drag events on the color picture RectTransform.
        private void InitializeColorPickerComponent()
        {
            colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
            colorPicker.onColorChange.RemoveAllListeners();

            //Don't let users apply a greyscale just yet. We have not quality tested the existing recoloring solution to know if it will behave as expected.
            colorPicker.saturationSlider.gameObject.SetActive(false);
            colorPicker.SetSaturation(1f);
        }

        //This the the actual color picker that renders on the screen.
        private void InitializeColorPickerElement()
        {
            GameObject colorPickerGameObject = playerSettingsPanel.RequireGameObject("ColorPicker");
            RectTransform colorPickerGameObjectTransform = (RectTransform)colorPickerGameObject.transform;
            colorPickerGameObjectTransform.anchorMin = new Vector2(0f, 0f);
            colorPickerGameObjectTransform.anchorMax = new Vector2(1f, 1f);
            colorPickerGameObjectTransform.pivot = new Vector2(0.5f, 0.5f);
            colorPickerGameObjectTransform.anchoredPosition = new Vector2(40f, 0f);
        }

        private static GUISkin GetGUISkin()
        {
            return GUISkinUtils.RegisterDerivedOnce("menus.serverPassword",
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

        private void DoServerPasswordWindow(int windowId)
        {
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
                            OnCancelClick();
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
}
