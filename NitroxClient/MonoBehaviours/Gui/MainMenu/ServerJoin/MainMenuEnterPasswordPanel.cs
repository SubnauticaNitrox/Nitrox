using System;
using System.Collections;
using System.Linq;
using FMODUnity;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
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

    public void FocusPasswordField()
    {
        StartCoroutine(Coroutine());

        IEnumerator Coroutine()
        {
            passwordInput.Select();
            EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
            yield return null;
            passwordInput.MoveToEndOfLine(false, true);
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
