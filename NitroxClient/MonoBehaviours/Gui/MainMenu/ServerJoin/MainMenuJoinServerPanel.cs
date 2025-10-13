using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using FMODUnity;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxModel.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UWE;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public class MainMenuJoinServerPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver, uGUI_IScrollReceiver, uGUI_IAdjustReceiver
{
    public const string NAME = "MultiplayerJoinServer";

    public static MainMenuJoinServerPanel Instance { get; private set; }

    private GameObject playerSettingsPanel;
    private TextMeshProUGUI header;
    private uGUI_ColorPicker colorPicker;
    private MainMenuColorPickerPreview colorPickerPreview;
    private Slider saturationSlider;
    private uGUI_InputField playerNameInputField;
    private TouchScreenKeyboard deckKeyboard;

    private GameObject selectedItem;
    private GameObject[] selectableItems;
    
    // Color picker controller state
    private bool isInColorPickerMode = false;
    private Image colorPanelHighlight;
    private Outline colorPanelBorder;

    public void Setup(GameObject savedGamesRef)
    {
        Instance = this;
        Destroy(transform.RequireGameObject("Scroll View"));
        Destroy(GetComponentInChildren<TranslationLiveUpdate>());
        header = GetComponentInChildren<TextMeshProUGUI>();

        CoroutineHost.StartCoroutine(AsyncSetup(savedGamesRef)); // As JoinServer waits for AsyncSetup to be completed we can't use normal Unity IEnumerator Start()
    }

    private IEnumerator AsyncSetup(GameObject savedGamesRef)
    {
        AsyncOperationHandle<GameObject> request = AddressablesUtility.LoadAsync<GameObject>("Assets/Prefabs/Base/GeneratorPieces/BaseMoonpoolUpgradeConsole.prefab");
        yield return request;
        GameObject colorPickerPanelPrototype = request.Result.RequireGameObject("EditScreen/Active");

        RectTransform parent = GetComponent<RectTransform>();

        GameObject newGameButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame/NewGameButton");
        LegendButtonData[] defaultLegend = newGameButtonRef.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration.Take(1).ToArray();

        //Create a clone of the RocketBase color picker panel.
        playerSettingsPanel = Instantiate(colorPickerPanelPrototype, parent);

        //Prepares name input field
        GameObject inputField = playerSettingsPanel.RequireGameObject("InputField");
        inputField.transform.SetParent(parent);
        inputField.transform.localPosition = new Vector3(-200, 310, 0);
        inputField.transform.localScale = Vector3.one;
        inputField.AddComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration = defaultLegend;
        playerNameInputField = inputField.GetComponent<uGUI_InputField>();
        ((TextMeshProUGUI)playerNameInputField.placeholder).text = Language.main.Get("Nitrox_EnterName");
        playerNameInputField.textComponent.fontSizeMin = 17;
        playerNameInputField.textComponent.fontSizeMax = 21;
        playerNameInputField.textComponent.GetComponent<RectTransform>().sizeDelta = new Vector2(-20, 42);
        playerNameInputField.characterLimit = 25; // See this.OnJoinClick()
        playerNameInputField.onFocusSelectAll = false;
        playerNameInputField.onSubmit.AddListener(_ => OnJoinClick());
        playerNameInputField.onSubmit.AddListener(_ => DeselectAllItems());
        playerNameInputField.ActivateInputField();

        //Prepares player color picker
        GameObject colorPickerObject = playerSettingsPanel.RequireGameObject("ColorPicker");
        colorPickerObject.transform.SetParent(parent);
        colorPickerObject.transform.localPosition = new Vector3(-268, 175, 0);
        colorPickerObject.transform.localScale = new Vector3(1.1f, 0.75f, 1);
        colorPicker = colorPickerObject.GetComponentInChildren<uGUI_ColorPicker>();
        colorPicker.pointer.localScale = new Vector3(1f, 1.46f, 1);
        saturationSlider = colorPicker.saturationSlider;
        saturationSlider.transform.localPosition = new Vector3(197, 0, 0);
        colorPickerPreview = colorPicker.gameObject.AddComponent<MainMenuColorPickerPreview>();
        colorPickerPreview.Init(colorPicker);

        // Create highlight border for controller mode
        GameObject highlightPanel = new GameObject("ColorPickerHighlight");
        highlightPanel.transform.SetParent(colorPickerObject.transform, false);
        highlightPanel.transform.SetAsFirstSibling(); // Put behind the color picker
        RectTransform highlightRect = highlightPanel.AddComponent<RectTransform>();
        highlightRect.anchorMin = new Vector2(0, 0);
        highlightRect.anchorMax = new Vector2(1, 1);
        highlightRect.offsetMin = new Vector2(-20, -20); // Padding around the color picker
        highlightRect.offsetMax = new Vector2(220, 20); // Include the saturation slider area
        
        // Create thin yellow border outline instead of filled rectangle
        colorPanelHighlight = highlightPanel.AddComponent<Image>();
        colorPanelHighlight.color = Color.clear; // Transparent fill
        colorPanelHighlight.raycastTarget = false; // Don't block input
        
        // Add Outline component for yellow border
        colorPanelBorder = highlightPanel.AddComponent<Outline>();
        colorPanelBorder.effectColor = Color.yellow; // Yellow border like button highlights
        colorPanelBorder.effectDistance = new Vector2(3, 3); // Thin border thickness
        colorPanelBorder.useGraphicAlpha = true;
        
        colorPanelHighlight.gameObject.SetActive(false); // Hidden by default

        GameObject buttonLeft = Instantiate(newGameButtonRef, parent);
        buttonLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 45);
        buttonLeft.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration = defaultLegend;
        GameObject buttonRight = Instantiate(buttonLeft, parent);

        //Prepares cancel button
        buttonLeft.transform.SetParent(parent);
        buttonLeft.transform.localPosition = new Vector3(-285, 40, 0);
        buttonLeft.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Cancel");
        Button cancelButton = buttonLeft.GetComponent<Button>();
        cancelButton.onClick = new Button.ButtonClickedEvent();
        cancelButton.onClick.AddListener(OnCancelClick);
        cancelButton.onClick.AddListener(DeselectAllItems);

        //Prepares join button
        buttonRight.transform.localPosition = new Vector3(-115, 40, 0);
        buttonRight.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Join");
        Button joinButton = buttonRight.GetComponent<Button>();
        joinButton.onClick = new Button.ButtonClickedEvent();
        joinButton.onClick.AddListener(OnJoinClick);
        joinButton.onClick.AddListener(DeselectAllItems);

        selectableItems = [inputField, colorPicker.gameObject, saturationSlider.gameObject, buttonLeft, buttonRight];
        Destroy(playerSettingsPanel);
    }

    private void OnJoinClick()
    {
        string playerName = playerNameInputField.text;

        //https://regex101.com/r/eTWiEs/2/
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9._-]{3,25}$"))
        {
            MainMenuNotificationPanel.ShowMessage(Language.main.Get("Nitrox_InvalidUserName"), NAME);
            return;
        }

        JoinServerBackend.RequestSessionReservation(playerName, colorPicker.currentColor);
    }

    private static void OnCancelClick()
    {
        JoinServerBackend.StopMultiplayerClient();
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
    }

    public void UpdatePanelValues(string serverName) => header.text = $"    {Language.main.Get("Nitrox_JoinServer")} {serverName}";

    public void UpdatePlayerPanelValues(string playerName, Vector3 hsb)
    {
        playerNameInputField.text = playerName;
        colorPicker.SetHSB(hsb);
    }

    public void FocusNameInputField()
    {
        StartCoroutine(Coroutine());
        IEnumerator Coroutine()
        {
            SelectFirstItem();
            yield return new WaitForEndOfFrame();
            playerNameInputField.MoveToEndOfLine(false, true);
            
            // Open onscreen keyboard for better controller compatibility
            // Use Steam Input keyboard when running through Proton/Steam
            bool shouldShowOSK = GameInput.PrimaryDevice == GameInput.Device.Controller ||
                                IsSteamInputActive() ||
                                UnityEngine.Input.GetJoystickNames().Length > 0;
            
            Log.Info($"OSK Debug - TouchScreen supported: {TouchScreenKeyboard.isSupported}, Controller: {GameInput.PrimaryDevice}, Steam: {IsSteamInputActive()}, Joysticks: {UnityEngine.Input.GetJoystickNames().Length}, ShouldShow: {shouldShowOSK}");
            
            if (shouldShowOSK)
            {
                Log.Info("Opening Steam Input keyboard for controller input");
                OpenSteamKeyboard();
            }
        }
    }

    public void OpenSteamKeyboard()
    {
        Log.Info("Attempting to open Steam Input keyboard");
        
        // For Proton/Steam Input, try multiple approaches to trigger Steam's OSK
        if (IsSteamInputActive())
        {
            // Method 1: Try Unity's TouchScreenKeyboard (may work through Steam Input layer)
            if (TouchScreenKeyboard.isSupported)
            {
                Log.Info("Using TouchScreenKeyboard API through Steam Input");
                deckKeyboard = TouchScreenKeyboard.Open(playerNameInputField.text, TouchScreenKeyboardType.Default, false, false, true, false);
            }
            
            // Method 2: Focus the input field strongly to trigger Steam Input's auto-keyboard
            Log.Info("Strongly focusing input field for Steam Input detection");
            playerNameInputField.ActivateInputField();
            playerNameInputField.Select();
            EventSystem.current.SetSelectedGameObject(playerNameInputField.gameObject);
            
            // Method 3: Simulate Steam Input keyboard hotkey (Steam+X on Steam Deck)
            // This might trigger Steam's virtual keyboard
            StartCoroutine(TriggerSteamKeyboardHotkey());
        }
        else
        {
            Log.Info("Steam Input not detected, falling back to standard TouchScreenKeyboard");
            if (TouchScreenKeyboard.isSupported)
            {
                deckKeyboard = TouchScreenKeyboard.Open(playerNameInputField.text, TouchScreenKeyboardType.Default, false, false, true, false);
            }
        }
    }
    
    private System.Collections.IEnumerator TriggerSteamKeyboardHotkey()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Try to trigger Steam's virtual keyboard by simulating the hotkey
        // Steam Deck uses Steam+X to open keyboard
        Log.Info("Attempting to trigger Steam virtual keyboard hotkey");
        
        // Keep the input field focused
        if (playerNameInputField != null)
        {
            playerNameInputField.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(playerNameInputField.gameObject);
        }
    }
    
    private System.Collections.IEnumerator RestoreCursorPosition(Vector3 originalPos)
    {
        yield return null; // Wait one frame for OSK to open
        
        // Only restore if we're still in a reasonable state
        if (UnityEngine.Input.mousePresent && Vector3.Distance(UnityEngine.Input.mousePosition, originalPos) > 50f)
        {
            // Unity doesn't allow direct mouse position setting, but we can at least 
            // ensure the UI state is consistent
            EventSystem.current.SetSelectedGameObject(playerNameInputField.gameObject);
        }
    }

    private bool IsHandheldDevice()
    {
        return SystemInfo.deviceModel.Contains("SteamDeck") || 
               System.Environment.GetEnvironmentVariable("SteamDeck") != null ||
               System.IO.File.Exists("/home/deck/.steampid") ||
               SystemInfo.deviceType == DeviceType.Handheld;
    }

    private bool IsSteamInputActive()
    {
        // Check for Steam environment variables that indicate Steam Input is active
        return System.Environment.GetEnvironmentVariable("SteamAppId") != null ||
               System.Environment.GetEnvironmentVariable("SteamGameId") != null ||
               System.Environment.GetEnvironmentVariable("STEAM_COMPAT_CLIENT_INSTALL_PATH") != null;
    }

    private void Update()
    {
        if (deckKeyboard != null)
        {
            // Keep field synced with OSK
            playerNameInputField.text = deckKeyboard.text;

            // Check if OSK closed
            if (!deckKeyboard.active || deckKeyboard.status == TouchScreenKeyboard.Status.Done || deckKeyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                deckKeyboard = null;
                // OSK closed, player can now use controller to navigate and join
            }
        }

        // Handle controller input for color picker
        HandleColorPickerControllerInput();
    }

    private void HandleColorPickerControllerInput()
    {
        // Only handle controller input if using controller
        bool isUsingController = GameInput.PrimaryDevice == GameInput.Device.Controller || 
                                IsSteamInputActive() || 
                                UnityEngine.Input.GetJoystickNames().Length > 0;
        
        if (!isUsingController)
        {
            return;
        }

        // Check if color picker is currently selected (any part of it)
        bool colorPickerSelected = selectedItem != null && 
                                  (selectedItem == colorPicker.gameObject || 
                                   selectedItem.transform.IsChildOf(colorPicker.transform) ||
                                   selectedItem.GetComponentInParent<uGUI_ColorPicker>() == colorPicker);

        // Update highlight visibility
        if (colorPanelHighlight != null)
        {
            colorPanelHighlight.gameObject.SetActive(colorPickerSelected);
        }

        // Handle color picker mode input
        if (isInColorPickerMode)
        {
            // Get joystick input - Left stick for color wheel, Right stick for saturation
            Vector2 leftStick = new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"));
            Vector2 rightStick = new Vector2(UnityEngine.Input.GetAxis("Joy X Axis 4"), UnityEngine.Input.GetAxis("Joy Y Axis 4"));

            // Handle left stick for color wheel (with deadzone)
            if (leftStick.magnitude > 0.1f)
            {
                HandleColorWheelController(leftStick);
            }

            // Handle right stick Y axis for saturation slider (with deadzone)
            if (Mathf.Abs(rightStick.y) > 0.1f)
            {
                HandleSaturationSliderController(-rightStick.y); // Invert Y for intuitive up/down control
            }

            // Exit color picker mode with B button or if color picker is no longer selected
            if (UnityEngine.Input.GetButtonDown("Cancel") || !colorPickerSelected)
            {
                ExitColorPickerMode();
            }
        }
    }
    
    /// <summary>
    /// VISUAL ENHANCEMENT - YELLOW BORDER COLOR PICKER:
    /// Activates controller mode for color picker with enhanced visual feedback.
    /// Uses Unity Outline component to create yellow border matching UI theme.
    /// Provides clear visual indication when color picker is active and ready for controller input.
    /// </summary>
    private void EnterColorPickerMode()
    {
        isInColorPickerMode = true;
        Log.Info("Entered color picker controller mode - use left stick for color wheel, right stick for intensity");
        
        // Make border more prominent when in color picker mode
        if (colorPanelBorder != null)
        {
            colorPanelBorder.effectColor = Color.yellow; // Bright yellow border
            colorPanelBorder.effectDistance = new Vector2(4, 4); // Thicker border when active
        }
    }
    
    private void ExitColorPickerMode()
    {
        isInColorPickerMode = false;
        Log.Info("Exited color picker controller mode");
        
        // Return border to normal selection state
        if (colorPanelBorder != null)
        {
            colorPanelBorder.effectColor = Color.yellow; // Normal yellow border
            colorPanelBorder.effectDistance = new Vector2(3, 3); // Normal border thickness
        }
        
        // End color picker preview
        if (GameInput.PrimaryDevice == GameInput.Device.Controller && colorPickerPreview != null)
        {
            colorPickerPreview.OnPointerUp(null);
        }
    }

    private void HandleColorWheelController(Vector2 stickInput)
    {
        if (colorPicker == null || colorPicker.pointer == null)
        {
            return;
        }

        // Get current pointer position relative to color wheel center
        Vector2 currentPos = colorPicker.pointer.localPosition;
        float wheelRadius = colorPicker.GetComponent<RectTransform>().rect.width * 0.5f * 0.85f; // Account for wheel bounds

        // Apply stick input to move the pointer
        Vector2 newPos = currentPos + stickInput * Time.unscaledDeltaTime * 100f; // Adjust speed as needed

        // Clamp to wheel radius
        if (newPos.magnitude > wheelRadius)
        {
            newPos = newPos.normalized * wheelRadius;
        }

        // Update pointer position
        colorPicker.pointer.localPosition = newPos;

        // Force color picker update by directly calling its color change mechanism
        // This simulates what happens when the user drags the pointer
        if (colorPickerPreview != null)
        {
            // The color picker preview component handles the color updates
            // Trigger a color update by calculating the color at the new position
            Vector2 normalizedPos = newPos / wheelRadius;
            float distance = Mathf.Clamp01(normalizedPos.magnitude);
            float angle = Mathf.Atan2(normalizedPos.y, normalizedPos.x) * Mathf.Rad2Deg;
            if (angle < 0)
            {
                angle += 360f;
            }
            
            // Convert to HSV color (using current saturation from slider)
            float hue = angle / 360f;
            float saturation = saturationSlider != null ? saturationSlider.value : 1f;
            Color newColor = Color.HSVToRGB(hue, saturation, 1f);
            
            // Trigger the color change event if it exists
            if (colorPicker.onColorChange != null)
            {
                ColorChangeEventData eventData = new ColorChangeEventData(EventSystem.current) 
                { 
                    color = newColor 
                };
                colorPicker.onColorChange.Invoke(eventData);
            }
        }
    }

    private void HandleSaturationSliderController(float rightStickY)
    {
        if (saturationSlider == null)
        {
            return;
        }

        // Get current slider value
        float currentValue = saturationSlider.value;
        
        // Apply right stick input to modify slider value
        float newValue = currentValue + rightStickY * Time.unscaledDeltaTime * 0.5f; // Adjust speed as needed
        
        // Clamp to slider bounds [0, 1]
        newValue = Mathf.Clamp01(newValue);
        
        // Update slider value
        saturationSlider.value = newValue;
    }

    public bool OnButtonDown(GameInput.Button button)
    {
        if (!selectedItem)
        {
            return false;
        }

        // Handle color picker mode
        if (button == GameInput.Button.UISubmit)
        {
            // Check if color picker is selected
            bool colorPickerSelected = selectedItem == colorPicker.gameObject || 
                                      selectedItem.transform.IsChildOf(colorPicker.transform) ||
                                      selectedItem.GetComponentInParent<uGUI_ColorPicker>() == colorPicker;
            
            if (colorPickerSelected && !isInColorPickerMode)
            {
                EnterColorPickerMode();
                return true;
            }
            else if (selectedItem.TryGetComponentInChildren(out TMP_InputField inputField))
            {
                inputField.Select();
                inputField.ActivateInputField();
                return true;
            }
            else if (selectedItem.TryGetComponentInChildren(out Button buttonComponent))
            {
                buttonComponent.onClick.Invoke();
                return true;
            }
        }
        else if (button == GameInput.Button.UICancel && isInColorPickerMode)
        {
            ExitColorPickerMode();
            return true;
        }
        
        return false;
    }

    public bool OnScroll(float scrollDelta, float speedMultiplier)
    {
        if (EventSystem.current != null &&
            EventSystem.current.currentSelectedGameObject == selectedItem &&
            selectedItem.TryGetComponent(out Slider slider))
        {
            slider.value += scrollDelta * speedMultiplier * 0.01f;
            return true;
        }

        return false;
    }

    public bool OnAdjust(Vector2 adjustDelta)
    {
        if (selectedItem && selectedItem.TryGetComponent(out uGUI_ColorPicker selectedColorPicker))
        {
            return selectedColorPicker.OnAdjust(adjustDelta);
        }

        return false;
    }

    object uGUI_INavigableIconGrid.GetSelectedItem() => selectedItem;

    bool uGUI_INavigableIconGrid.ShowSelector => false;
    bool uGUI_INavigableIconGrid.EmulateRaycast => false;
    bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos) => false;
    uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY) => null;
    Graphic uGUI_INavigableIconGrid.GetSelectedIcon() => null;

    public void SelectItem(object item)
    {
        DeselectItem();
        selectedItem = item as GameObject;

        if (!selectedItem)
        {
            return;
        }

        if (selectedItem.TryGetComponent(out mGUI_Change_Legend_On_Select changeLegend))
        {
            changeLegend.SyncLegendBarToGUISelection();
        }
        else
        {
            uGUI_LegendBar.ClearButtons();
        }

        if (selectedItem == selectableItems[1])
        {
            colorPicker.pointer.GetComponent<Image>().color = Color.cyan;
            if (GameInput.PrimaryDevice == GameInput.Device.Controller)
            {
                colorPickerPreview.OnPointerDown(null);
            }
        }
        else if (selectedItem == selectableItems[3] || selectedItem == selectableItems[4])
        {
            selectedItem.GetComponentInChildren<uGUI_BasicColorSwap>().makeTextBlack();
        }

        if (selectedItem.TryGetComponentInChildren(out Selectable selectable))
        {
            selectable.Select();
        }

        // Open onscreen keyboard when player name field is selected via controller
        if ((GameInput.PrimaryDevice == GameInput.Device.Controller || IsSteamInputActive()) && 
            selectedItem == selectableItems[0])
        {
            OpenSteamKeyboard();
        }

        if (!EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(selectedItem);
        }

        RuntimeManager.PlayOneShot(MainMenuServerListPanel.HoverSound.path);
    }

    public void DeselectItem()
    {
        if (!selectedItem)
        {
            return;
        }

        // Exit color picker mode if we're deselecting the color picker
        bool wasColorPickerSelected = selectedItem == colorPicker.gameObject || 
                                     selectedItem.transform.IsChildOf(colorPicker.transform) ||
                                     selectedItem.GetComponentInParent<uGUI_ColorPicker>() == colorPicker;
        
        if (wasColorPickerSelected && isInColorPickerMode)
        {
            ExitColorPickerMode();
        }

        if (selectedItem.TryGetComponent(out TMP_InputField selectedInputField))
        {
            //This line need to be before selectedInputField.ReleaseSelection() as it will call this method recursive leading to NRE
            selectedInputField.DeactivateInputField();
            selectedInputField.ReleaseSelection();
        }
        else if (selectedItem.TryGetComponent(out uGUI_ColorPicker selectedColorPicker))
        {
            Image colorPickerPointer = selectedColorPicker.pointer.GetComponent<Image>();

            if (colorPickerPointer.color != Color.white &&
                GameInput.PrimaryDevice == GameInput.Device.Controller)
            {
                colorPickerPreview.OnPointerUp(null);
            }
            colorPickerPointer.color = Color.white;
        }
        else if (selectedItem.TryGetComponentInChildren(out uGUI_BasicColorSwap colorSwap))
        {
            colorSwap.makeTextWhite();
        }

        if (!EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        selectedItem = null;
    }

    public void DeselectAllItems()
    {
        foreach (GameObject item in selectableItems)
        {
            selectedItem = item;
            DeselectItem();
        }
    }

    public bool SelectFirstItem()
    {
        SelectItem(selectableItems[0]);
        return true;
    }

    public bool SelectItemInDirection(int dirX, int dirY)
    {
        if (!selectedItem)
        {
            return SelectFirstItem();
        }

        if (selectedItem == selectableItems[0]) //Name input
        {
            switch (dirY)
            {
                case < 0:
                    SelectItem(selectableItems[^2]);
                    return true;
                case > 0:
                    SelectItem(selectableItems[1]);
                    return true;
            }
        }

        if (selectedItem == selectableItems[1] || selectedItem == selectableItems[2]) // ColorPicker and SaturationSlider
        {
            switch (dirY)
            {
                case < 0:
                    SelectItem(selectableItems[0]);
                    return true;
                case > 0:
                    SelectItem(selectableItems[3]);
                    return true;
            }

            if (dirX != 0)
            {
                int direction = selectedItem == selectableItems[1] ? 0 : 1;
                direction = (direction + dirX) % 2;

                SelectItem(selectableItems[1 + direction]);
                return true;
            }
        }

        if (selectedItem == selectableItems[3] || selectedItem == selectableItems[4]) // CancelButton and ConfirmButton
        {
            switch (dirY)
            {
                case < 0:
                    SelectItem(selectableItems[1]);
                    return true;
                case > 0:
                    SelectItem(selectableItems[0]);
                    return true;
            }

            if (dirX != 0)
            {
                int direction = selectedItem == selectableItems[3] ? 0 : 1;
                direction = (direction + dirX) % 2;

                SelectItem(selectableItems[3 + direction]);
                return true;
            }
        }

        return false;
    }
}
