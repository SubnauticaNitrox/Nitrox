using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using FMODUnity;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;
using NitroxClient.Unity.Helper;
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

    private GameObject selectedItem;
    private GameObject[] selectableItems;

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
        }
    }

    public bool OnButtonDown(GameInput.Button button)
    {
        if (button != GameInput.Button.UISubmit || !selectedItem)
        {
            return false;
        }

        if (selectedItem.TryGetComponentInChildren(out TMP_InputField inputField))
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
        else if (selectedItem.TryGetComponentInChildren(out Button buttonComponent))
        {
            buttonComponent.onClick.Invoke();
        }
        return true;
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
            if (GameInput.GetPrimaryDevice() == GameInput.Device.Controller)
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
            //This line need to be before selectedInputField.ReleaseSelection() as it will call this method recursive leading to NRE
            selectedInputField.DeactivateInputField();
            selectedInputField.ReleaseSelection();
        }
        else if (selectedItem.TryGetComponent(out uGUI_ColorPicker selectedColorPicker))
        {
            Image colorPickerPointer = selectedColorPicker.pointer.GetComponent<Image>();

            if (colorPickerPointer.color != Color.white &&
                GameInput.GetPrimaryDevice() == GameInput.Device.Controller)
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
