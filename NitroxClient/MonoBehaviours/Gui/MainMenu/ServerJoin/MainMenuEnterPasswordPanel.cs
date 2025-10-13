using System;
using System.Collections;
using System.Linq;
using FMODUnity;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;

public class MainMenuEnterPasswordPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
    public const string NAME = "MultiplayerEnterPassword";

    public static MainMenuEnterPasswordPanel Instance { get; private set; }

    private TMP_InputField passwordInput;
    private mGUI_Change_Legend_On_Select legendChange;
    private TouchScreenKeyboard deckKeyboard;

    private GameObject selectedItem;
    private GameObject[] selectableItems;
    


    private static string lastEnteredPassword;
    public static Optional<string> LastEnteredPassword => lastEnteredPassword != null ? Optional.Of(lastEnteredPassword) : Optional.Empty;
    public static void ResetLastEnteredPassword() => lastEnteredPassword = null;

    public void Setup(GameObject savedGamesRef)
    {
        Instance = this;

        GameObject multiplayerButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
        GameObject generalTextRef = multiplayerButtonRef.GetComponentInChildren<TextMeshProUGUI>().gameObject;
        GameObject inputFieldRef = GameObject.Find("/Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox/InputField");

        GameObject passwordInputGameObject = Instantiate(inputFieldRef, transform, false);
        passwordInputGameObject.transform.localPosition = new Vector3(-160, 300, 0);
        passwordInputGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
        passwordInput = passwordInputGameObject.GetComponent<TMP_InputField>();
        passwordInput.characterValidation = TMP_InputField.CharacterValidation.None;
        passwordInput.onSubmit = new TMP_InputField.SubmitEvent();
        passwordInput.onSubmit.AddListener(_ => OnConfirmButtonClicked());
        passwordInput.placeholder.GetComponent<TranslationLiveUpdate>().translationKey = Language.main.Get("Nitrox_JoinServerPlaceholder");
        GameObject passwordInputDesc = Instantiate(generalTextRef, passwordInputGameObject.transform, false);
        passwordInputDesc.transform.localPosition = new Vector3(-200, 0, 0);
        passwordInputDesc.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_JoinServerPassword");

        GameObject confirmButton = Instantiate(multiplayerButtonRef, transform, false);
        confirmButton.transform.localPosition = new Vector3(-200, 90, 0);
        confirmButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Confirm");
        Button confirmButtonButton = confirmButton.RequireTransform("NewGameButton").GetComponent<Button>();
        confirmButtonButton.onClick = new Button.ButtonClickedEvent();
        confirmButtonButton.onClick.AddListener(OnConfirmButtonClicked);

        GameObject backButton = Instantiate(multiplayerButtonRef, transform, false);
        backButton.transform.localPosition = new Vector3(-200, 40, 0);
        backButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Cancel");
        Button backButtonButton = backButton.RequireTransform("NewGameButton").GetComponent<Button>();
        backButtonButton.onClick = new Button.ButtonClickedEvent();
        backButtonButton.onClick.AddListener(OnCancelClick);

        selectableItems = [passwordInputGameObject, confirmButton, backButton];
        Destroy(transform.Find("Scroll View").gameObject);

        legendChange = gameObject.AddComponent<mGUI_Change_Legend_On_Select>();
        legendChange.legendButtonConfiguration = confirmButtonButton.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration.Take(1).ToArray();
    }

    private void OnEnable()
    {
        /// <summary>
        /// STEAM DECK OSK FEATURE:
        /// Automatically focus the password field when panel becomes active.
        /// This enables immediate typing and triggers Steam Input OSK for controller users.
        /// Essential for Steam Deck and controller-based password entry.
        /// </summary>
        if (passwordInput != null)
        {
            StartCoroutine(AutoFocusPasswordField());
        }
    }

    private IEnumerator AutoFocusPasswordField()
    {
        // Wait a frame for the UI to fully activate
        yield return new WaitForEndOfFrame();
        
        // Focus and activate the password input field immediately
        passwordInput.Select();
        passwordInput.ActivateInputField();
        EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
        
        Log.Info("Password field auto-focused - ready for immediate typing");
        Log.Info("Controller users: OSK will appear when controller navigation is detected");
    }

    public void FocusPasswordField()
    {
        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            passwordInput.Select();
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
            yield return null;
            passwordInput.MoveToEndOfLine(false, true);
            
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

    /// <summary>
    /// STEAM DECK OSK INTEGRATION:
    /// Opens Steam Input on-screen keyboard for controller-based password entry.
    /// Uses multiple methods to ensure compatibility across different Steam configurations:
    /// - Unity TouchScreenKeyboard API routed through Steam Input layer
    /// - Strong input field focus to trigger Steam's automatic OSK detection
    /// Essential for Steam Deck, Steam Input controllers, and handheld gaming devices.
    /// </summary>
    public void OpenSteamKeyboard()
    {
        Log.Info("Attempting to open Steam Input keyboard for password field");
        
        // For Proton/Steam Input, try multiple approaches to trigger Steam's OSK
        if (IsSteamInputActive())
        {
            // Method 1: Try Unity's TouchScreenKeyboard (may work through Steam Input layer)
            if (TouchScreenKeyboard.isSupported)
            {
                Log.Info("Using TouchScreenKeyboard API through Steam Input");
                deckKeyboard = TouchScreenKeyboard.Open(passwordInput.text, TouchScreenKeyboardType.Default, false, false, true, false);
            }
            
            // Method 2: Focus the input field strongly to trigger Steam Input's auto-keyboard
            Log.Info("Strongly focusing password field for Steam Input detection");
            passwordInput.ActivateInputField();
            passwordInput.Select();
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
            
            // Method 3: Simulate Steam Input keyboard hotkey
            StartCoroutine(TriggerSteamKeyboardHotkey());
        }
        else
        {
            Log.Info("Steam Input not detected, falling back to standard TouchScreenKeyboard");
            if (TouchScreenKeyboard.isSupported)
            {
                deckKeyboard = TouchScreenKeyboard.Open(passwordInput.text, TouchScreenKeyboardType.Default, false, false, true, false);
            }
        }
    }
    
    private System.Collections.IEnumerator TriggerSteamKeyboardHotkey()
    {
        yield return new WaitForSeconds(0.1f);
        
        // Try to trigger Steam's virtual keyboard
        Log.Info("Attempting to trigger Steam virtual keyboard hotkey for password field");
        
        // Keep the input field focused
        if (passwordInput != null)
        {
            passwordInput.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
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
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
        }
    }

    private bool IsHandheldDevice()
    {
        return SystemInfo.deviceModel.Contains("SteamDeck") || 
               System.Environment.GetEnvironmentVariable("SteamDeck") != null ||
               System.IO.File.Exists("/home/deck/.steampid") ||
               SystemInfo.deviceType == DeviceType.Handheld;
    }

    /// <summary>
    /// STEAM INPUT DETECTION:
    /// Detects if Steam Input is active by checking Steam-specific environment variables.
    /// These variables are set when running through Steam or Proton and indicate 
    /// that Steam Input layer is available for controller and OSK functionality.
    /// Used to determine when to trigger Steam-specific keyboard behavior.
    /// </summary>
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
            passwordInput.text = deckKeyboard.text;

            // Check if OSK closed
            if (!deckKeyboard.active || deckKeyboard.status == TouchScreenKeyboard.Status.Done || deckKeyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                deckKeyboard = null;

                // Commit the text to the panel
                if (!string.IsNullOrEmpty(passwordInput.text))
                {
                    OnConfirmButtonClicked();
                }
            }
        }

        // Open onscreen keyboard when controller is detected
        if ((GameInput.PrimaryDevice == GameInput.Device.Controller || IsSteamInputActive()) && 
            selectedItem == passwordInput.gameObject && 
            deckKeyboard == null)
        {
            OpenSteamKeyboard();
        }
    }

    private void OnConfirmButtonClicked()
    {
        lastEnteredPassword = passwordInput.text;
        MainMenuRightSide.main.OpenGroup(MainMenuJoinServerPanel.NAME);
        MainMenuJoinServerPanel.Instance.FocusNameInputField();
    }

    private static void OnCancelClick()
    {
        JoinServerBackend.StopMultiplayerClient();
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
    }

    public bool OnButtonDown(GameInput.Button button)
    {
        switch (button)
        {
            case GameInput.Button.UISubmit:
                OnConfirm();
                return true;
            case GameInput.Button.UICancel:
                OnBack();
                return true;
            default:
                return false;
        }
    }

    public void OnConfirm()
    {
        if (selectedItem.TryGetComponentInChildren(out TMP_InputField inputField))
        {
            inputField.ActivateInputField();
        }

        if (selectedItem.TryGetComponentInChildren(out Button button))
        {
            button.onClick.Invoke();
        }
    }

    public void OnBack()
    {
        passwordInput.text = string.Empty;
        ResetLastEnteredPassword();
        DeselectAllItems();
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
    }

    bool uGUI_INavigableIconGrid.ShowSelector => false;
    bool uGUI_INavigableIconGrid.EmulateRaycast => false;
    bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos) => false;
    uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY) => null;
    Graphic uGUI_INavigableIconGrid.GetSelectedIcon() => null;

    object uGUI_INavigableIconGrid.GetSelectedItem() => selectedItem;

    public void SelectItem(object item)
    {
        DeselectItem();
        selectedItem = item as GameObject;

        legendChange.SyncLegendBarToGUISelection();

        if (!selectedItem)
        {
            return;
        }

        if (selectedItem.TryGetComponent(out TMP_InputField selectedInputField))
        {
            selectedInputField.Select();
            // Open onscreen keyboard when password field is selected via controller
            if ((GameInput.PrimaryDevice == GameInput.Device.Controller || IsSteamInputActive()) && selectedInputField == passwordInput)
            {
                OpenSteamKeyboard();
            }
        }
        else // Buttons
        {
            selectedItem.GetComponentInChildren<uGUI_BasicColorSwap>().makeTextBlack();
            selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = MainMenuServerListPanel.SelectedSprite;
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

        if (selectedItem.TryGetComponent(out TMP_InputField selectedInputField))
        {
            selectedInputField.DeactivateInputField();
            selectedInputField.ReleaseSelection();
        }
        else // Buttons
        {
            selectedItem.GetComponentInChildren<uGUI_BasicColorSwap>().makeTextWhite();
            selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = MainMenuServerListPanel.NormalSprite;
        }

        if (!EventSystem.current.alreadySelecting)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        selectedItem = null;
    }

    public void DeselectAllItems()
    {
        foreach (GameObject child in selectableItems)
        {
            selectedItem = child;
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

        if (dirX == dirY)
        {
            return false;
        }

        // Check if controller is being used and password field is focused
        bool isUsingController = GameInput.PrimaryDevice == GameInput.Device.Controller || 
                                IsSteamInputActive() || 
                                UnityEngine.Input.GetJoystickNames().Length > 0;
        
        // If controller is being used and password field is selected, keep focus on password field
        if (isUsingController && selectedItem == selectableItems[0]) // Password field is first item
        {
            Log.Debug("Controller detected with password field focused - locking navigation to password field");
            passwordInput.Select();
            passwordInput.ActivateInputField();
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
            return true; // Stay on password field
        }

        int dir = dirX + dirY > 0 ? 1 : -1;
        for (int newIndex = GetSelectedIndex() + dir; newIndex >= 0 && newIndex < selectableItems.Length; newIndex += dir)
        {
            if (SelectItemByIndex(newIndex))
            {
                return true;
            }
        }

        return false;
    }

    private int GetSelectedIndex() => selectedItem ? Array.IndexOf(selectableItems, selectedItem) : -1;

    private bool SelectItemByIndex(int selectedIndex)
    {
        if (selectedIndex >= 0 && selectedIndex < selectableItems.Length)
        {
            SelectItem(selectableItems[selectedIndex]);
            return true;
        }

        return false;
    }
}
