using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FMODUnity;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;

public class MainMenuServerListPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
    public const string NAME = "MultiplayerServerList";

    public static MainMenuServerListPanel Main;
    public static Sprite NormalSprite;
    public static Sprite SelectedSprite;
    public static FMODAsset HoverSound;

    private GameObject multiplayerNewServerButtonRef;
    private GameObject multiplayerServerButtonRef;
    private Transform serverAreaContent;
    private GameObject selectedServerItem;
    private ScrollRect scrollRect;
    private GameObject scrollBar;

    public bool IsJoining { get; set; }

    public void Setup(GameObject savedGamesRef)
    {
        Main = this;

        MainMenuLoadMenu loadMenu = savedGamesRef.GetComponentInChildren<MainMenuLoadMenu>();
        NormalSprite = loadMenu.normalSprite;
        SelectedSprite = loadMenu.selectedSprite;
        HoverSound = loadMenu.hoverSound;

        multiplayerNewServerButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");
        serverAreaContent = transform.RequireTransform("Scroll View/Viewport/SavedGameAreaContent");
        serverAreaContent.gameObject.name = "ServerAreaContent";
        serverAreaContent.GetComponent<GridLayoutGroup>().spacing = new Vector2(0, 5);

        scrollRect = transform.RequireGameObject("Scroll View").GetComponent<ScrollRect>();
        scrollBar = scrollRect.RequireGameObject("Scrollbar");

        multiplayerServerButtonRef = savedGamesRef.GetComponent<MainMenuLoadPanel>().saveInstance;
        MainMenuServerButton.Setup(multiplayerServerButtonRef.GetComponent<MainMenuLoadButton>());

        CreateAddServerButton();
        LoadSavedServers();
        FindLANServersAsync().ContinueWith(t =>
        {
            if (t is { IsFaulted: true, Exception: { } ex })
            {
                Log.Warn($"Failed to execute FindLANServersAsync: {ex.GetFirstNonAggregateMessage()}");
            }
        });
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
            MainMenuRightSide.main.OpenGroup(MainMenuCreateServerPanel.NAME);
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

        if (!selectedServerItem)
        {
            return;
        }

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

    public bool SelectItemByIndex(int selectedIndex)
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

    private async Task FindLANServersAsync()
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

    public GameObject CreateServerButton(string serverName, string address, int port, bool isReadOnly = false)
    {
        GameObject multiplayerButtonInst = Instantiate(multiplayerServerButtonRef, serverAreaContent, false);
        multiplayerButtonInst.name = $"NitroxServer_{serverAreaContent.childCount - 2}";
        DestroyImmediate(multiplayerButtonInst.RequireGameObject("Load")); // Needs to be deleted before MainMenuServerButton.Init() below
        Destroy(multiplayerButtonInst.GetComponent<MainMenuLoadButton>());

        GameObject multiplayerLoadButtonInst = Instantiate(multiplayerNewServerButtonRef, multiplayerButtonInst.transform, false);
        multiplayerLoadButtonInst.name = "Load";

        StringBuilder buttonText = new();
        buttonText.Append(Language.main.Get("Nitrox_ConnectTo")).Append(" <b>").Append(serverName).AppendLine("</b>");
        if (NitroxPrefs.HideIp.Value)
        {
            buttonText.AppendLine("***.***.***.***").AppendLine("*****");
        }
        else
        {
            buttonText.Append(address[^Math.Min(address.Length, 25)..]).Append(':').Append(port);
        }

        MainMenuServerButton serverButton = multiplayerButtonInst.AddComponent<MainMenuServerButton>();
        serverButton.Init(buttonText.ToString(), address, port, isReadOnly);

        scrollBar.SetActive(serverAreaContent.childCount >= 4);
        foreach (EventTrigger eventTrigger in multiplayerButtonInst.GetComponentsInChildren<EventTrigger>(true))
        {
            ForwardTriggerScrollToScrollRect(eventTrigger);
        }

        return multiplayerButtonInst;
    }

    private void CreateAddServerButton()
    {
        GameObject multiplayerButtonInst = Instantiate(multiplayerNewServerButtonRef, serverAreaContent, false);
        multiplayerButtonInst.name = "NewServer"; // "NewServer" is important, see OnConfirm()
        Transform txt = multiplayerButtonInst.RequireTransform("NewGameButton/Text");
        txt.GetComponent<TextMeshProUGUI>().text = "Nitrox_AddServer";

        Button multiplayerButtonButton = multiplayerButtonInst.RequireTransform("NewGameButton").GetComponent<Button>();
        multiplayerButtonButton.onClick = new Button.ButtonClickedEvent();
        multiplayerButtonButton.onClick.AddListener(OpenAddServerGroup);

        ForwardTriggerScrollToScrollRect(multiplayerButtonButton.GetComponent<EventTrigger>());
    }

    private void ForwardTriggerScrollToScrollRect(EventTrigger eventTrigger)
    {
        eventTrigger.triggers.RemoveAll(trigger => trigger.eventID == EventTriggerType.Scroll);

        EventTrigger.TriggerEvent callback = new();
        callback.AddListener(x => scrollRect.Scroll(((PointerEventData)x).scrollDelta.y, 5f));

        eventTrigger.triggers.Add(new EventTrigger.Entry
        {
            eventID = EventTriggerType.Scroll,
            callback = callback
        });
    }

    public void OpenAddServerGroup()
    {
        DeselectAllItems();
        MainMenuRightSide.main.OpenGroup(MainMenuCreateServerPanel.NAME);
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
        FindLANServersAsync().ContinueWith(t =>
        {
            if (t is { IsFaulted: true, Exception: { } ex })
            {
                Log.Warn($"Failed to execute FindLANServersAsync: {ex.GetFirstNonAggregateMessage()}");
            }
        });
    }
}
