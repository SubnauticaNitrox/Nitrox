using System;
using System.Collections;
using System.Diagnostics;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.ColorSwap;
using NitroxClient.GameLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.GameLogic.PlayerPreferences;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.MultiplayerSession;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        private static readonly GameObject colorPickerPanelPrototype = Resources.Load<GameObject>("WorldEntities/Tools/RocketBase")
            .RequireGameObject("Base/BuildTerminal/GUIScreen/CustomizeScreen/Panel/");

        private PlayerPreference activePlayerPreference;
        private bool isSubscribed;
        private GameObject joinServerMenu;
        private GameObject multiplayerClient;
        private IMultiplayerSession multiplayerSession;
        private GameObject playerSettingsPanel;
        private PlayerPreferenceManager preferencesManager;
        public string ServerIp = "";
        public int ServerPort;
        public static GameObject SaveGameMenuPrototype { get; set; }

        private static MainMenuRightSide RightSideMainMenu => MainMenuRightSide.main;

        public void Awake()
        {
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();
            preferencesManager = NitroxServiceLocator.LocateService<PlayerPreferenceManager>();

            InitializeJoinMenu();
            SubscribeColorChanged();

            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            //Set Server IP in info label
            GameObject lowerDetailTextGameObject = playerSettingsPanel.RequireGameObject("LowerDetail/Text");
            lowerDetailTextGameObject.GetComponent<Text>().text = $"Server IP Address\n{ServerIp}";

            //Initialize elements from preferences
            activePlayerPreference = preferencesManager.GetPreference(ServerIp);

            float hue;
            float saturation;
            float vibrancy;

            Color playerColor = new Color(activePlayerPreference.RedAdditive, activePlayerPreference.GreenAdditive, activePlayerPreference.BlueAdditive);

            Color.RGBToHSV(playerColor, out hue, out saturation, out vibrancy);
            uGUI_ColorPicker colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
            colorPicker.SetHSB(new Vector3(hue, 1f, vibrancy));

            GameObject playerNameInputFieldGameObject = playerSettingsPanel.RequireGameObject("InputField");

            uGUI_InputField playerNameInputField = playerNameInputFieldGameObject.GetComponent<uGUI_InputField>();
            playerNameInputField.text = activePlayerPreference.PlayerName;

            StartMultiplayerClient();
        }

        public void Update()
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

        public void OnDestroy()
        {
            UnsubscribeColorChanged();
            if (joinServerMenu != null)
            {
                MainMenuGroup group = joinServerMenu.GetComponent<MainMenuGroup>();
                if (group)
                {
                    RightSideMainMenu.groups.Remove(group);
                }
            }
            Destroy(joinServerMenu);
        }

        private static GameObject CloneSaveGameMenuPrototype()
        {
            GameObject joinServerMenu = Instantiate(SaveGameMenuPrototype);
            Destroy(joinServerMenu.RequireGameObject("Header"));
            Destroy(joinServerMenu.RequireGameObject("Scroll View"));
            Destroy(joinServerMenu.GetComponent<LayoutGroup>());
            Destroy(joinServerMenu.GetComponent<MainMenuLoadPanel>());
            joinServerMenu.GetAllComponentsInChildren<LayoutGroup>().ForEach(Destroy);

            //We cannot register click events on child transforms if they are being captured here.
            joinServerMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

            return joinServerMenu;
        }

        private static GameObject CloneColorPickerPanelPrototype()
        {
            //Create a clone of the RocketBase color picker panel.
            GameObject playerSettingsPanel = Instantiate(colorPickerPanelPrototype);
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

            return playerSettingsPanel;
        }

        //This panel acts as the parent of all other UI elements on the menu. It is parented by the cloned "SaveGame" menu.
        private static void InitializePlayerSettingsPanelElement(RectTransform joinServerBackground, GameObject playerSettingsPanel)
        {
            playerSettingsPanel.SetActive(true);

            RectTransform playerSettingsPanelTransform = (RectTransform)playerSettingsPanel.transform;
            playerSettingsPanelTransform.SetParent(joinServerBackground, false);
            playerSettingsPanelTransform.anchorMin = new Vector2(0f, 0f);
            playerSettingsPanelTransform.anchorMax = new Vector2(1f, 1f);
            playerSettingsPanelTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        //The base tab is the outline surrounding the color picker, as well as teh "Player Color" label and associated "Selected Color" image.
        private static void InitializeBaseTabElement(RectTransform joinServerBackground, GameObject playerSettingsPanel)
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

            Text baseTabText = baseTabTextGameObject.GetComponent<Text>();
            baseTabText.text = "Player Color";

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
        private static void InitializeLowerDetailElement(GameObject playerSettingsPanel)
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
            GameObject lowerDetailTextGameObject = lowerDetailRectTransform.RequireGameObject("Text");
            Text lowerDetailText = lowerDetailTextGameObject.GetComponent<Text>();
            lowerDetailText.resizeTextForBestFit = true;
            lowerDetailText.alignment = TextAnchor.MiddleCenter;

            RectTransform lowerDetailTextRectTransform = (RectTransform)lowerDetailTextGameObject.transform;
            lowerDetailTextRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.pivot = new Vector2(0.5f, 0.5f);
            lowerDetailTextRectTransform.anchoredPosition = new Vector2(0, 0);
        }

        //Player name textbox
        private static void InitializePlayerNameInputElement(GameObject playerSettingsPanel)
        {
            GameObject playerNameInputFieldGameObject = playerSettingsPanel.RequireGameObject("InputField");
            RectTransform inputFieldRectTransform = (RectTransform)playerNameInputFieldGameObject.transform;
            inputFieldRectTransform.anchoredPosition = new Vector2(inputFieldRectTransform.anchoredPosition.x, inputFieldRectTransform.anchoredPosition.y - 15f);

            uGUI_InputField playerNameInputField = playerNameInputFieldGameObject.GetComponent<uGUI_InputField>();
            playerNameInputField.selectionColor = Color.white;

            GameObject inputFieldPlaceholder = inputFieldRectTransform.RequireGameObject("Placeholder");
            Text inputFieldPlaceholderText = inputFieldPlaceholder.GetComponent<Text>();
            inputFieldPlaceholderText.text = "Enter Player Name";
        }

        //This is the "service" that manages the click and drag events on the color picture RectTransform.
        private static void InitializeColorPickerComponent(GameObject playerSettingsPanel)
        {
            uGUI_ColorPicker colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
            colorPicker.onColorChange.RemoveAllListeners();

            //Don't let users apply a grayscale just yet. We have not quality tested the existing recoloring solution to know if it will behave as expected.
            colorPicker.saturationSlider.gameObject.SetActive(false);
            colorPicker.SetSaturation(1f);
        }

        //This the the actual color picker that renders on the screen.
        private static void InitializeColorPickerElement(GameObject playerSettingsPanel)
        {
            GameObject colorPickerGameObject = playerSettingsPanel.RequireGameObject("ColorPicker");
            RectTransform colorPickerGameObjectTransform = (RectTransform)colorPickerGameObject.transform;
            colorPickerGameObjectTransform.anchorMin = new Vector2(0f, 0f);
            colorPickerGameObjectTransform.anchorMax = new Vector2(1f, 1f);
            colorPickerGameObjectTransform.pivot = new Vector2(0.5f, 0.5f);
            colorPickerGameObjectTransform.anchoredPosition = new Vector2(40f, 0f);
        }

        private void SubscribeColorChanged()
        {
            if (isSubscribed)
            {
                return;
            }

            uGUI_ColorPicker colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
            colorPicker.onColorChange.AddListener(OnColorChange);

            isSubscribed = true;
        }

        private void UnsubscribeColorChanged()
        {
            if (playerSettingsPanel == null || !isSubscribed)
            {
                return;
            }

            uGUI_ColorPicker colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
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
            GameObject playerNameInputFieldGameObject = playerSettingsPanel.RequireGameObject("InputField");
            uGUI_InputField playerNameInputField = playerNameInputFieldGameObject.GetComponent<uGUI_InputField>();

            playerNameInputField.ActivateInputField();
        }

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject();
                multiplayerClient.name = "Multiplayer Client";
                multiplayerClient.AddComponent<Multiplayer>();
                multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
            }

            try
            {
                multiplayerSession.Connect(ServerIp, ServerPort);
            }
            catch (ClientConnectionFailedException)
            {
                Log.InGame($"Unable to contact the remote server at: {ServerIp}:{ServerPort}");

                if (ServerIp.Equals("127.0.0.1"))
                {
                    if (Process.GetProcessesByName("NitroxServer-Subnautica").Length == 0)
                    {
                        Log.InGame("Start your server first to join your self-hosted world");
                    }
                    else
                    {
                        Log.InGame("Seems like your firewall settings are interfering");
                    } 
                }

                OnCancelClick();
            }
        }

        private void OnCancelClick()
        {
            StopMultiplayerClient();
            RightSideMainMenu.OpenGroup("Multiplayer");
            gameObject.SetActive(false);
        }

        private void OnJoinClick()
        {
            Text playerNameText = playerSettingsPanel.RequireTransform("InputField/Text").GetComponent<Text>();

            string playerName = playerNameText.text;

            if (string.IsNullOrEmpty(playerName))
            {
                NotifyUser("Survival is a systemic initiative, but even the lowliest of cogs needs a designation in order to effectively coordinate the collective effort towards a - desireable, outcome.\n\n" +
                           "Please identify yourself so that your presence may be indexed with local Survivor PDA telemetry instruments...");
                return;
            }

            uGUI_ColorPicker colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
            Color playerColor = colorPicker.currentColor;

            SetCurrentPreference(playerName, playerColor);

            PlayerSettings playerSettings = new PlayerSettings(playerColor);
            AuthenticationContext authenticationContext = new AuthenticationContext(playerName);

            multiplayerSession.RequestSessionReservation(playerSettings, authenticationContext);
        }

        private void SetCurrentPreference(string playerName, Color playerColor)
        {
            PlayerPreference newPreference = new PlayerPreference(playerName, playerColor);

            if (activePlayerPreference.Equals(newPreference))
            {
                return;
            }

            preferencesManager.SetPreference(ServerIp, newPreference);
        }

        private void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.ESTABLISHING_SERVER_POLICY:
                    Log.InGame("Requesting session policy information...");
                    break;
                case MultiplayerSessionConnectionStage.AWAITING_RESERVATION_CREDENTIALS:
                    Log.InGame("Waiting for User Input...");

                    RightSideMainMenu.OpenGroup("Join Server");
                    FocusPlayerNameTextbox();

                    break;
                case MultiplayerSessionConnectionStage.SESSION_RESERVED:
                    Log.InGame("Launching game...");

                    multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                    preferencesManager.Save();

#pragma warning disable CS0618 // God Damn it UWE...
                    IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
#pragma warning restore CS0618 // God damn it UWE...
                    StartCoroutine(startNewGame);
                    LoadingScreenVersionText.Initialize();

                    break;
                case MultiplayerSessionConnectionStage.SESSION_RESERVATION_REJECTED:
                    Log.InGame("Reservation rejected...");

                    MultiplayerSessionReservationState reservationState = multiplayerSession.Reservation.ReservationState;

                    string reservationRejectionNotification = reservationState.Describe();

                    NotifyUser(
                        reservationRejectionNotification,
                        () =>
                        {
                            multiplayerSession.Disconnect();
                            multiplayerSession.Connect(ServerIp, ServerPort);
                        });

                    break;
                case MultiplayerSessionConnectionStage.DISCONNECTED:
                    Log.Info("Disconnected from server");
                    break;
            }
        }

        private void NotifyUser(string notificationMessage, Action continuationAction = null)
        {
            if (gameObject.GetComponent<MainMenuNotification>() != null)
            {
                return;
            }

            Action wrappedAction = () =>
            {
                continuationAction?.Invoke();
                Destroy(gameObject.GetComponent<MainMenuNotification>(), 0.0001f);
            };

            MainMenuNotification notificationDialog = gameObject.AddComponent<MainMenuNotification>();
            notificationDialog.ShowNotification(notificationMessage, wrappedAction);
        }

        private void StopMultiplayerClient()
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

        //This method merges the cloned color picker element with the existing template for menus that appear in the "right side" region of Subnautica's main menu.
        private void InitializeJoinMenu()
        {
            GameObject joinServerMenu = CloneSaveGameMenuPrototype();
            joinServerMenu.name = "Join Server";

            joinServerMenu.transform.SetParent(RightSideMainMenu.transform, false);
            RightSideMainMenu.groups.Add(joinServerMenu.GetComponent<MainMenuGroup>());

            //Not sure what is up with this menu, but we have to use the RectTransform of the Image component as the parent for our color picker panel.
            //Most of the UI elements seem to vanish behind this Image otherwise.
            RectTransform joinServerBackground = joinServerMenu.GetComponent<Image>().rectTransform;
            joinServerBackground.anchorMin = new Vector2(0.5f, 0.5f);
            joinServerBackground.anchorMax = new Vector2(0.5f, 0.5f);
            joinServerBackground.pivot = new Vector2(0.5f, 0.5f);
            joinServerBackground.anchoredPosition = new Vector2(joinServerBackground.anchoredPosition.x, 5f);

            InitializePlayerSettingsPanel(joinServerBackground);

            this.joinServerMenu = joinServerMenu;
        }

        //This configures and re-positions the elements on the default "ColorGrayscale" menu to suite our purposes now.
        private void InitializePlayerSettingsPanel(RectTransform joinServerBackground)
        {
            GameObject playerSettingsPanel = CloneColorPickerPanelPrototype();

            InitializePlayerSettingsPanelElement(joinServerBackground, playerSettingsPanel);
            InitializeBaseTabElement(joinServerBackground, playerSettingsPanel);
            InitializeLowerDetailElement(playerSettingsPanel);
            InitializePlayerNameInputElement(playerSettingsPanel);
            InitializeColorPickerComponent(playerSettingsPanel);
            InitializeColorPickerElement(playerSettingsPanel);
            InitializeButtonElements(joinServerBackground, playerSettingsPanel);

            this.playerSettingsPanel = playerSettingsPanel;
        }

        //Join and Cancel buttons
        private void InitializeButtonElements(RectTransform joinServerBackground, GameObject playerSettingsPanel)
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
            Text cancelButtonText = cancelButtonTextGameObject.GetComponent<Text>();
            cancelButtonText.text = "Cancel";

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
            Text joinButtonText = joinButtonTextGameObject.GetComponent<Text>();
            joinButtonText.text = "Join";

            //Flip the text so it is no longer upside down after flipping the button.
            RectTransform joinButtonTextRectTransform = (RectTransform)joinButtonTextGameObject.transform;
            joinButtonTextRectTransform.Rotate(Vector3.forward * -180);
        }
    }
}
