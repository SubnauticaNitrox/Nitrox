using System;
using System.Collections;
using System.Linq;
using FMODUnity;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;

public class MainMenuCreateServerPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
    public const string NAME = "MultiplayerCreateServer";
    private const string DEFAULT_PORT = "11000";

    private TMP_InputField serverNameInput, serverAddressInput, serverPortInput;
    private mGUI_Change_Legend_On_Select legendChange;

    private GameObject selectedItem;
    private GameObject[] selectableItems;

    public void Setup(GameObject savedGamesRef)
    {
        GameObject multiplayerButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
        GameObject generalTextRef = multiplayerButtonRef.GetComponentInChildren<TextMeshProUGUI>().gameObject;

        GameObject inputFieldRef = GameObject.Find("/Menu canvas/Panel/MainMenu/RightSide/Home/EmailBox/InputField");
        GameObject inputFieldBlueprint = Instantiate(inputFieldRef, transform, false);

        inputFieldBlueprint.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 40);
        TMP_InputField inputFieldBlueprintInput = inputFieldBlueprint.GetComponent<TMP_InputField>();
        inputFieldBlueprintInput.characterValidation = TMP_InputField.CharacterValidation.None;
        inputFieldBlueprintInput.onSubmit = new TMP_InputField.SubmitEvent();
        inputFieldBlueprintInput.onSubmit.AddListener(_ => { SelectItemInDirection(0, 1); });

        GameObject serverName = Instantiate(inputFieldBlueprint, transform, false);
        serverName.transform.localPosition = new Vector3(-160, 300, 0);
        serverNameInput = serverName.GetComponent<TMP_InputField>();
        serverNameInput.placeholder.GetComponent<TranslationLiveUpdate>().translationKey = Language.main.Get("Nitrox_AddServer_NamePlaceholder");
        GameObject serverNameDesc = Instantiate(generalTextRef, serverName.transform, false);
        serverNameDesc.transform.localPosition = new Vector3(-200, 0, 0);
        serverNameDesc.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_AddServer_NameDescription");

        GameObject serverAddress = Instantiate(inputFieldBlueprint, transform, false);
        serverAddress.transform.localPosition = new Vector3(-160, 225, 0);
        serverAddressInput = serverAddress.GetComponent<TMP_InputField>();
        serverAddressInput.placeholder.GetComponent<TranslationLiveUpdate>().translationKey = Language.main.Get("Nitrox_AddServer_AddressPlaceholder");
        GameObject serverAddressDesc = Instantiate(generalTextRef, serverAddress.transform, false);
        serverAddressDesc.transform.localPosition = new Vector3(-200, 0, 0);
        serverAddressDesc.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_AddServer_AddressDescription");

        GameObject serverPort = Instantiate(inputFieldBlueprint, transform, false);
        serverPort.transform.localPosition = new Vector3(-160, 150, 0);
        serverPortInput = serverPort.GetComponent<TMP_InputField>();
        serverPortInput.characterValidation = TMP_InputField.CharacterValidation.Integer;
        serverPortInput.placeholder.GetComponent<TranslationLiveUpdate>().translationKey = Language.main.Get("Nitrox_AddServer_PortPlaceholder");
        serverPortInput.text = DEFAULT_PORT;
        GameObject serverPortDesc = Instantiate(generalTextRef, serverPort.transform, false);
        serverPortDesc.transform.localPosition = new Vector3(-200, 0, 0);
        serverPortDesc.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_AddServer_PortDescription");

        GameObject confirmButton = Instantiate(multiplayerButtonRef, transform, false);
        confirmButton.transform.localPosition = new Vector3(-200, 90, 0);
        confirmButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        confirmButton.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_AddServer_Confirm");
        Button confirmButtonButton = confirmButton.RequireTransform("NewGameButton").GetComponent<Button>();
        confirmButtonButton.onClick = new Button.ButtonClickedEvent();
        confirmButtonButton.onClick.AddListener(SaveServer);

        GameObject backButton = Instantiate(multiplayerButtonRef, transform, false);
        backButton.transform.localPosition = new Vector3(-200, 40, 0);
        backButton.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Cancel");
        Button backButtonButton = backButton.RequireTransform("NewGameButton").GetComponent<Button>();
        backButtonButton.onClick = new Button.ButtonClickedEvent();
        backButtonButton.onClick.AddListener(OnBack);

        selectableItems = [serverName, serverAddress, serverPort, confirmButton, backButton];
        Destroy(inputFieldBlueprint);
        Destroy(transform.Find("Scroll View").gameObject);

        legendChange = gameObject.AddComponent<mGUI_Change_Legend_On_Select>();
        legendChange.legendButtonConfiguration = confirmButtonButton.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration.Take(2).ToArray();
    }

    private void SaveServer()
    {
        string serverNameText = serverNameInput.text.Trim();
        string serverHostText = serverAddressInput.text.Trim();
        string serverPortText = serverPortInput.text.Trim();

        if (string.IsNullOrWhiteSpace(serverNameText) ||
            string.IsNullOrWhiteSpace(serverHostText) ||
            string.IsNullOrWhiteSpace(serverPortText) ||
            !int.TryParse(serverPortText, out int serverPort))
        {
            Log.InGame(Language.main.Get("Nitrox_AddServer_InvalidInput"));
            return;
        }

        GameObject newEntry = MainMenuServerListPanel.Main.CreateServerButton(serverNameText, serverHostText, serverPort);
        ServerList.Instance.Add(new ServerList.Entry(serverNameText, serverHostText, serverPort));
        ServerList.Instance.Save();
        OnBack();
        MainMenuServerListPanel.Main.StartCoroutine(DelayedScrollToNewEntry());
        Log.InGame(Language.main.Get("Nitrox_AddServer_CreatedSuccessful"));
        return;

        IEnumerator DelayedScrollToNewEntry()
        {
            yield return new WaitForEndOfFrame();
            UIUtils.ScrollToShowItemInCenter(newEntry.transform);
        }
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

    private void Update()
    {
        if (GameInput.GetKeyDown(KeyCode.Tab))
        {
            if (GameInput.GetKey(KeyCode.LeftShift))
            {
                SelectItemInDirection(-1, 0);
            }
            else
            {
                SelectItemInDirection(1, 0);
            }
        }
        else if (selectedItem && GameInput.GetKeyDown(KeyCode.Return))
        {
            OnConfirm();
        }
    }

    public void OnBack()
    {
        serverNameInput.text = string.Empty;
        serverAddressInput.text = string.Empty;
        serverPortInput.text = string.Empty;
        DeselectAllItems();
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
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

        legendChange.SyncLegendBarToGUISelection();

        if (!selectedItem)
        {
            return;
        }

        if (selectedItem.TryGetComponent(out TMP_InputField selectedInputField))
        {
            selectedInputField.Select();
        }
        else // Button
        {
            selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = MainMenuServerListPanel.SelectedSprite;
            selectedItem.GetComponentInChildren<uGUI_BasicColorSwap>().makeTextBlack();
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
        else // Button
        {
            selectedItem.transform.GetChild(0).GetComponent<Image>().sprite = MainMenuServerListPanel.NormalSprite;
            selectedItem.transform.GetChild(0).GetComponent<uGUI_BasicColorSwap>().makeTextWhite();
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
