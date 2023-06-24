using System.Net;
using FMODUnity;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public class MainMenuServerListPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
    public static MainMenuServerListPanel Main;
    public static Sprite NormalSprite;
    public static Sprite SelectedSprite;
    public static FMODAsset HoverSound;

    private GameObject multiplayerButtonRef;
    private Transform serverAreaContent;
    private GameObject selectedServerItem;

    public JoinServer JoinServer { get; private set; }
    public bool IsJoining { get; set; }

    public void Setup(GameObject savedGamesRef)
    {
        Main = this;

        //This sucks, but the only way around it is to establish a Subnautica resources cache and reference it everywhere we need it.
        //Given recent push-back on elaborate designs, I've just crammed it here until we can all get on the same page as far as code-quality standards are concerned.
        JoinServer = new GameObject("NitroxJoinServer").AddComponent<JoinServer>();
        JoinServer.Setup(savedGamesRef);

        MainMenuLoadMenu loadMenu = savedGamesRef.GetComponentInChildren<MainMenuLoadMenu>();
        NormalSprite = loadMenu.normalSprite;
        SelectedSprite = loadMenu.selectedSprite;
        HoverSound = loadMenu.hoverSound;

        multiplayerButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
        serverAreaContent = transform.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");
        serverAreaContent.gameObject.name = "ServerAreaContent";

        MainMenuLoadButton loadButtonRef = savedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance.GetComponent<MainMenuLoadButton>();
        MainMenuServerButton.Setup(loadButtonRef);

        CreateAddServerButton();
        LoadSavedServers();
        _ = FindLANServersAsync();
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
            case GameInput.Button.UIClear:
                OnClear();
                return true;
            default:
                return false;
        }
    }

    public void OnBack()
    {
        DeselectAllItems();
        MainMenuRightSide.main.OpenGroup("Home");
    }

    public void OnClear()
    {
        if (selectedServerItem && selectedServerItem.TryGetComponent(out MainMenuServerButton serverButton))
        {
            serverButton.RequestDelete();
        }
    }

    public void OnConfirm()
    {
        if (!selectedServerItem)
        {
            return;
        }

        if (selectedServerItem.gameObject.name == "NewServer")
        {
            DeselectAllItems();
            MainMenuRightSide.main.OpenGroup("MultiplayerCreateServer");
        }
        else if (selectedServerItem.TryGetComponent(out MainMenuServerButton serverButton))
        {
            serverButton.OnJoinButtonClicked();
        }
    }

    object uGUI_INavigableIconGrid.GetSelectedItem() => selectedServerItem;

    bool uGUI_INavigableIconGrid.ShowSelector => false;

    bool uGUI_INavigableIconGrid.EmulateRaycast => false;
    bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos) => false;
    uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY) => null;

    Graphic uGUI_INavigableIconGrid.GetSelectedIcon() => null;

    public void SelectItem(object item)
    {
        DeselectItem();
        selectedServerItem = item as GameObject;

        if (selectedServerItem.TryGetComponentInChildren(out mGUI_Change_Legend_On_Select componentInChildren))
        {
            componentInChildren.SyncLegendBarToGUISelection();
        }

        UIUtils.ScrollToShowItemInCenter(selectedServerItem.transform);

        selectedServerItem.transform.GetChild(0).GetComponent<Image>().sprite = SelectedSprite;
        selectedServerItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
        RuntimeManager.PlayOneShot(HoverSound.path);
    }

    public void DeselectItem()
    {
        if (!selectedServerItem)
        {
            return;
        }

        selectedServerItem.transform.GetChild(0).GetComponent<Image>().sprite = NormalSprite;
        selectedServerItem.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        selectedServerItem = null;
    }

    public void DeselectAllItems()
    {
        foreach (Transform child in serverAreaContent)
        {
            child.GetChild(0).GetComponent<Image>().sprite = NormalSprite;
            child.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
        }
    }

    public bool SelectFirstItem()
    {
        MainMenuServerButton firstServerObject = serverAreaContent.GetComponentInChildren<MainMenuServerButton>();
        if (firstServerObject)
        {
            SelectItem(firstServerObject.gameObject);
            return true;
        }

        Transform serverCreationButton = serverAreaContent.GetChild(0);
        if (serverCreationButton && serverCreationButton.name == "NewServer")
        {
            SelectItem(serverCreationButton.gameObject);
            return true;
        }

        return false;
    }

    public bool SelectItemInDirection(int dirX, int dirY)
    {
        if (selectedServerItem)
        {
            return dirY != 0 && SelectItemInYDirection(GetSelectedIndex(), dirY);
        }

        return SelectFirstItem();
    }

    public int GetSelectedIndex() => selectedServerItem ? selectedServerItem.transform.GetSiblingIndex() : -1;

    public bool SelectItemInYDirection(int selectedIndex, int dirY)
    {
        int dir = dirY > 0 ? 1 : -1;
        for (int newIndex = selectedIndex + dir; newIndex >= 0 && newIndex < serverAreaContent.childCount; newIndex += dir)
        {
            if (SelectItemByIndex(newIndex))
            {
                return true;
            }
        }

        return false;
    }

    private bool SelectItemByIndex(int selectedIndex)
    {
        if (selectedIndex < serverAreaContent.childCount && selectedIndex >= 0)
        {
            SelectItem(serverAreaContent.GetChild(selectedIndex).gameObject);
            return true;
        }

        return false;
    }

    private void LoadSavedServers()
    {
        ServerList.Refresh();
        foreach (ServerList.Entry entry in ServerList.Instance.Entries)
        {
            CreateServerButton(entry.Name, entry.Address, entry.Port);
        }
    }

    private async System.Threading.Tasks.Task FindLANServersAsync()
    {
        void AddButton(IPEndPoint serverEndPoint)
        {
            // Add ServerList entry to keep indices in sync with servers UI, to enable removal by index
            ServerList.Instance.Add(new("LAN Server", $"{serverEndPoint.Address}", $"{serverEndPoint.Port}", false));
            CreateServerButton("LAN Server", serverEndPoint.Address.ToString(), serverEndPoint.Port, true);
        }

        LANBroadcastClient.ServerFound += AddButton;
        await LANBroadcastClient.SearchAsync();
        LANBroadcastClient.ServerFound -= AddButton;
    }

    public void CreateServerButton(string serverName, string address, int port, bool isReadOnly = false)
    {
        string HideIfNecessary(object text) => NitroxPrefs.HideIp.Value ? "****" : $"{text}";

        GameObject multiplayerButtonInst = Instantiate(multiplayerButtonRef, serverAreaContent, false);
        multiplayerButtonInst.name = $"NitroxServer_{serverAreaContent.childCount - 1}";

        string text = $"{Language.main.Get("Nitrox_ConnectTo")} <b>{serverName}</b>\n{HideIfNecessary(address)}:{HideIfNecessary(port)}";
        MainMenuServerButton serverButton = multiplayerButtonInst.AddComponent<MainMenuServerButton>();
        serverButton.Init(text, address, port, isReadOnly);
    }

    private void CreateAddServerButton()
    {
        GameObject multiplayerButtonInst = Instantiate(multiplayerButtonRef, serverAreaContent, false);
        multiplayerButtonInst.name = "NewServer"; // "NewServer" is important, see OnConfirm()
        Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
        txt.GetComponent<TextMeshProUGUI>().text = "Nitrox_AddServer";

        Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
        multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
        multiplayerButtonButton.onClick.AddListener(OpenAddServerGroup);
    }

    public void OpenAddServerGroup()
    {
        DeselectAllItems();
        MainMenuRightSide.main.OpenGroup("MultiplayerCreateServer");
    }

    public void RefreshServerEntries()
    {
        if (!serverAreaContent)
        {
            return;
        }

        foreach (Transform child in serverAreaContent)
        {
            Destroy(child.gameObject);
        }

        CreateAddServerButton();
        LoadSavedServers();
        _ = FindLANServersAsync();
    }
}
