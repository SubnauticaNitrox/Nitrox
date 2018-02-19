using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.GameLogic.PlayerModelBuilder;
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
        //This is really expensive and creates a noticeable lag in the UI if loaded on-demand. 
        //I suggest doing it this way to front-load this performance hit - which should result in a more attractive UX.
        private static readonly GameObject colorPickerPanelPrototype = Resources.Load<GameObject>("WorldEntities/Tools/RocketBase")
            .transform
            .Find("Base/BuildTerminal/GUIScreen/CustomizeScreen/Panel/")
            .gameObject;

        public static GameObject SaveGameMenuPrototype { get; set; }

        private static MainMenuRightSide RightSideMainMenu => MainMenuRightSide.main;
        private GameObject joinServerMenu;
        private GameObject playerSettingsPanel;
        private bool isSubscribed;
        private GameObject multiplayerClient;
        private IMultiplayerSession multiplayerSession;
        private bool notifyingUnableToJoin;
        private bool shouldFocus;
        private Rect unableToJoinWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        private string username = "username";

        public string ServerIp = "";

        public void Awake()
        {
            multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSession>();

            InitializeJoinMenu();
            SubscribeColorChanged();
        }

        public void Start()
        {
            GameObject lowerDetailTextGameObject = playerSettingsPanel.transform.Find("LowerDetail/Text").gameObject;
            lowerDetailTextGameObject.GetComponent<Text>().text = $"Server IP Address\n{ServerIp}";
            StartMultiplayerClient();
        }

        public void OnDestroy()
        {
            UnsubscribeColorChanged();
            RightSideMainMenu.groups.Remove(joinServerMenu);
            Destroy(joinServerMenu);
        }

        public void OnGUI()
        {
            if (notifyingUnableToJoin)
            {
                unableToJoinWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), unableToJoinWindowRect, RenderUnableToJoinDialog, "Unable to Join Session");
            }
        }

        public void OnColorChange(ColorChangeEventData eventData)
        {
            Color selectedColor = eventData.color;

            GameObject selectedColorGameObject = playerSettingsPanel.transform
                .Find("BaseTab/SelectedColor")
                .gameObject;

            Image baseTabSelectedColorImage = selectedColorGameObject.GetComponent<Image>();
            baseTabSelectedColorImage.color = selectedColor;
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

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject();
                multiplayerClient.AddComponent<Multiplayer>();
                multiplayerSession.ConnectionStateChanged += SessionConnectionStateChangedHandler;
            }

            multiplayerSession.Connect(ServerIp);
        }

        private void SessionConnectionStateChangedHandler(IMultiplayerSessionConnectionState state)
        {
            switch (state.CurrentStage)
            {
                case MultiplayerSessionConnectionStage.EstablishingServerPolicy:
                    Log.InGame("Requesting session policy information...");
                    break;
                case MultiplayerSessionConnectionStage.AwaitingReservationCredentials:
                    RightSideMainMenu.OpenGroup("Join Server");
                    break;
                case MultiplayerSessionConnectionStage.SessionReserved:
                    Log.InGame("Launching game...");

                    multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
                    StartCoroutine(LaunchSession());

                    break;
                case MultiplayerSessionConnectionStage.SessionReservationRejected:
                    Log.InGame("Reservation rejected...");
                    notifyingUnableToJoin = true;
                    break;
                case MultiplayerSessionConnectionStage.Disconnected:
                    Log.Info("Disconnected from server");
                    break;
            }
        }

        private void StopMultiplayerClient()
        {
            if (multiplayerClient != null)
            {
                Multiplayer.Main.StopCurrentSession();
                Destroy(multiplayerClient);
                multiplayerClient = null;
                multiplayerSession.ConnectionStateChanged -= SessionConnectionStateChangedHandler;
            }
        }

        private IEnumerator LaunchSession()
        {
            Log.InGame("Launching game...");

#pragma warning disable CS0618 // Type or member is obsolete
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
            StartCoroutine(startNewGame);

            Log.InGame("Waiting for game to load...");
            // Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);

            Log.InGame("Joining Multiplayer Session...");
            Multiplayer.Main.StartSession();

            Destroy(gameObject);
        }

        private void RenderJoinServerDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        multiplayerSession.RequestSessionReservation(new PlayerSettings(RandomColorGenerator.GenerateColor()), new AuthenticationContext(username));
                        break;
                    case KeyCode.Escape:
                        StopMultiplayerClient();
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin("menus.server", 80), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Username:");
                        GUI.SetNextControlName("usernameField");
                        username = GUILayout.TextField(username);
                    }

                    if (GUILayout.Button("Join"))
                    {
                        multiplayerSession.RequestSessionReservation(new PlayerSettings(RandomColorGenerator.GenerateColor()), new AuthenticationContext(username));
                    }

                    if (GUILayout.Button("Cancel"))
                    {
                        StopMultiplayerClient();
                    }
                }
            });

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }

        private void RenderUnableToJoinDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        notifyingUnableToJoin = false;
                        break;
                    case KeyCode.Escape:
                        notifyingUnableToJoin = false;
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin("dialogs.server.rejected", 490), () =>
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        MultiplayerSessionReservationState reservationState = multiplayerSession.Reservation.ReservationState;
                        string reservationStateDescription = reservationState.Describe();

                        GUILayout.Label(reservationStateDescription);
                    }

                    if (GUILayout.Button("OK"))
                    {
                        notifyingUnableToJoin = false;
                        multiplayerSession.Disconnect();
                        multiplayerSession.Connect(ServerIp);
                    }
                }
            });
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

        //This method merges the cloned color picker element with the existing template for menus that appear in the "right side" region of Subnautica's main menu.
        private void InitializeJoinMenu()
        {
            GameObject joinServerMenu = CloneSaveGameMenuPrototype();
            joinServerMenu.name = "Join Server";

            joinServerMenu.transform.SetParent(RightSideMainMenu.transform, false);
            RightSideMainMenu.groups.Add(joinServerMenu);

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

        private static GameObject CloneSaveGameMenuPrototype()
        {
            GameObject joinServerMenu = Instantiate(SaveGameMenuPrototype);

            Destroy(joinServerMenu.transform.Find("Header").gameObject);
            Destroy(joinServerMenu.transform.Find("Scrollbar").gameObject);
            Destroy(joinServerMenu.transform.Find("SavedGameArea").gameObject);
            Destroy(joinServerMenu.GetComponent<LayoutGroup>());
            Destroy(joinServerMenu.GetComponent<MainMenuLoadPanel>());
            joinServerMenu.GetAllComponentsInChildren<LayoutGroup>().ForEach(Destroy);

            //We cannot register click events on child transforms if they are being captured here.
            joinServerMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;

            return joinServerMenu;
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

        private static GameObject CloneColorPickerPanelPrototype()
        {
            //Create a clone of the RocketBase color picker panel.
            GameObject playerSettingsPanel = Instantiate(colorPickerPanelPrototype);
            GameObject baseTab = playerSettingsPanel.transform.Find("BaseTab").gameObject;
            GameObject serverNameLabel = playerSettingsPanel.transform.Find("Name Label").gameObject;
            GameObject stripe1Tab = playerSettingsPanel.transform.Find("Stripe1Tab").gameObject;
            GameObject stripe2Tab = playerSettingsPanel.transform.Find("Stripe2Tab").gameObject;
            GameObject nameTab = playerSettingsPanel.transform.Find("NameTab").gameObject;
            GameObject frontOverlay = playerSettingsPanel.transform.Find("FrontOverlay").gameObject;
            GameObject colorLabel = playerSettingsPanel.transform.Find("Color Label").gameObject;

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
            GameObject baseTab = playerSettingsPanel.transform.Find("BaseTab").gameObject;

            //Reposition the border
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
            GameObject baseTabSelectedColor = baseTabTransform.Find("SelectedColor").gameObject;
            Image baseTabSelectedColorImage = baseTabSelectedColor.GetComponent<Image>();
            baseTabSelectedColorImage.rectTransform.anchoredPosition = new Vector2(
                baseTabSelectedColorImage.rectTransform.anchoredPosition.x + (originalBaseTabWidth - baseTabTransform.rect.width) / 2,
                baseTabSelectedColorImage.rectTransform.anchoredPosition.y);

            //Place the "Player Color" label to the right of the SelectedColor image and shrink it to fit the new tab region.
            GameObject baseTabTextGameObject = baseTabTransform.Find("Text").gameObject;
            RectTransform baseTabTextTransform = (RectTransform)baseTabTextGameObject.transform;
            baseTabTextTransform.anchorMin = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.anchorMax = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.pivot = new Vector2(0.5f, 0.5f);
            baseTabTextTransform.sizeDelta = new Vector2(80, 35);

            baseTabTextTransform.anchoredPosition = new Vector2(
                baseTabSelectedColorImage.rectTransform.anchoredPosition.x + (baseTabTextTransform.rect.width / 2f) + 22f,
                baseTabSelectedColorImage.rectTransform.anchoredPosition.y);

            Text baseTabText = baseTabTextGameObject.GetComponent<Text>();
            baseTabText.text = "Player Color";

            //This resizes the actual Image that outlines all of the UI elements.
            GameObject baseTabBackgroundGameObject = baseTabTransform.Find("Background").gameObject;
            Image baseTabBackgroundImage = baseTabBackgroundGameObject.GetComponent<Image>();
            baseTabBackgroundImage.rectTransform.sizeDelta = new Vector2(joinServerBackground.rect.width, baseTabTransform.rect.height);

            //This removes the extra "tabs" from the base texture.
            Texture2D newBaseTabTexture = baseTabBackgroundImage.sprite.texture.Clone();
            HsvColorFilter baseTabBackgroundColorFilter = new HsvColorFilter(-1f, -1f, -1f, 0f);
            baseTabBackgroundColorFilter.AddHueRange(185f, 215f);
            baseTabBackgroundColorFilter.AddAlphaRange(0f, 175f);
            newBaseTabTexture.ApplyFiltersToBlock(3, 3, 160, (int)(baseTabBackgroundImage.sprite.textureRect.height - 71f), baseTabBackgroundColorFilter);
            baseTabBackgroundImage.sprite = Sprite.Create(newBaseTabTexture, new Rect(baseTabBackgroundImage.sprite.textureRect), new Vector2(0f, 0f));
        }

        //The LowerDetail is the region that displays the current Server IP and the graphic that appears beneath it.
        private static void InitializeLowerDetailElement(GameObject playerSettingsPanel)
        {
            GameObject lowerDetail = playerSettingsPanel.transform.Find("LowerDetail").gameObject;

            //We use this as a reference point for positioning the LowerDetail element.
            RectTransform baseTabTextTransform = (RectTransform)playerSettingsPanel.transform.Find("BaseTab/Text");

            RectTransform lowerDetailRectTransform = (RectTransform)lowerDetail.transform;
            lowerDetailRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.pivot = new Vector2(0.5f, 0.5f);
            lowerDetailRectTransform.anchoredPosition = new Vector2(baseTabTextTransform.anchoredPosition.x - 24f, baseTabTextTransform.anchoredPosition.y - 61.4f);

            //The text element is right-aligned by default and needs to be centered for our purposes
            GameObject lowerDetailTextGameObject = lowerDetailRectTransform.Find("Text").gameObject;
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
            GameObject playerNameInputField = playerSettingsPanel.transform.Find("InputField").gameObject;
            RectTransform inputFieldRectTransform = (RectTransform)playerNameInputField.transform;
            inputFieldRectTransform.anchoredPosition = new Vector2(inputFieldRectTransform.anchoredPosition.x, inputFieldRectTransform.anchoredPosition.y - 15f);

            GameObject inputFieldPlaceholder = inputFieldRectTransform.Find("Placeholder").gameObject;
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
            GameObject colorPickerGameObject = playerSettingsPanel.transform.Find("ColorPicker").gameObject;
            RectTransform colorPickerGameObjectTransform = (RectTransform)colorPickerGameObject.transform;
            colorPickerGameObjectTransform.anchorMin = new Vector2(0f, 0f);
            colorPickerGameObjectTransform.anchorMax = new Vector2(1f, 1f);
            colorPickerGameObjectTransform.pivot = new Vector2(0.5f, 0.5f);
            colorPickerGameObjectTransform.anchoredPosition = new Vector2(40f, 0f);
        }

        //Join and Cancel buttons
        private static void InitializeButtonElements(RectTransform joinServerBackground, GameObject playerSettingsPanel)
        {
            GameObject cancelButton = playerSettingsPanel.transform.Find("Button").gameObject;
            GameObject joinButton = Instantiate(cancelButton, playerSettingsPanel.transform, false);

            RectTransform cancelButtonTransform = (RectTransform)cancelButton.transform;
            GameObject cancelButtonTextGameObject = cancelButtonTransform.Find("Text").gameObject;
            Text cancelButtonText = cancelButtonTextGameObject.GetComponent<Text>();
            cancelButtonText.text = "Cancel";

            cancelButtonTransform.sizeDelta = new Vector2(cancelButtonTransform.rect.width * 0.85f, cancelButtonTransform.rect.height);
            cancelButtonTransform.anchoredPosition = new Vector2(
                -1f * joinServerBackground.rect.width / 2f + cancelButtonTransform.rect.width / 2f,
                -1f * (joinServerBackground.rect.height / 2f) + cancelButtonTransform.rect.height / 2f + 3f);

            RectTransform joinButtonTransform = (RectTransform)joinButton.transform;
            joinButtonTransform.anchoredPosition = new Vector2(
                joinServerBackground.rect.width / 2f - joinButtonTransform.rect.width / 2f + 20f,
                -1f * (joinServerBackground.rect.height / 2f) + joinButtonTransform.rect.height / 2f + 3f);

            //Flip the button over
            joinButtonTransform.sizeDelta = new Vector2(joinButtonTransform.rect.width * 0.85f, joinButtonTransform.rect.height);
            joinButtonTransform.Rotate(Vector3.forward * -180);

            GameObject joinButtonTextGameObject = joinButtonTransform.Find("Text").gameObject;
            Text joinButtonText = joinButtonTextGameObject.GetComponent<Text>();
            joinButtonText.text = "Join";
            
            //Flip the text so it is no longer upside down after flipping the button.
            RectTransform joinButtonTextRectTransform = (RectTransform)joinButtonTextGameObject.transform;
            joinButtonTextRectTransform.Rotate(Vector3.forward * -180);
        }
    }
}
