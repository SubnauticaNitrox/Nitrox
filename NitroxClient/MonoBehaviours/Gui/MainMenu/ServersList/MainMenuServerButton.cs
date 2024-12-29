using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.GameLogic.Settings;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;

public class MainMenuServerButton : MonoBehaviour
{
    private static MainMenuLoadButton loadButtonRef;
    private static LegendButtonData[] confirmButtonLegendData;
    private static GameObject deleteButtonRef;

    private CanvasGroup loadCg;
    private CanvasGroup deleteCg;
    private Button cancelDeleteButton;

    private string joinIp;
    private int joinPort;
    private string joinServerName;

    public static void Setup(MainMenuLoadButton _loadButtonRef)
    {
        loadButtonRef = _loadButtonRef;
        confirmButtonLegendData = _loadButtonRef.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration;
        deleteButtonRef = _loadButtonRef.deleteButton;
    }

    public void Init(string serverName, string ip, int port, bool isReadOnly)
    {
        joinIp = ip;
        joinPort = port;
        joinServerName = serverName;

        Transform loadTransform = this.RequireTransform("Load");
        loadCg = loadTransform.gameObject.AddComponent<CanvasGroup>();
        Transform newGameButtonTransform = loadTransform.RequireTransform("NewGameButton");

        TextMeshProUGUI tmp = newGameButtonTransform.RequireTransform("Text").GetComponent<TextMeshProUGUI>();
        Destroy(tmp.GetComponent<TranslationLiveUpdate>());
        StringBuilder buttonText = new(Language.main.Get("Nitrox_ConnectTo"));
        buttonText.Append(" <b>").Append(serverName).AppendLine("</b>");
        if (NitroxPrefs.HideIp.Value)
        {
            buttonText.AppendLine("***.***.***.***:*****");
        }
        else
        {
            buttonText.Append(ip[^Math.Min(ip.Length, 25)..]).Append(':').Append(port);
        }
        tmp.text = buttonText.ToString();

        Button multiplayerJoinButton = newGameButtonTransform.GetComponent<Button>();
        multiplayerJoinButton.onClick = new Button.ButtonClickedEvent();
        multiplayerJoinButton.onClick.AddListener(() => _ = OnJoinButtonClicked());

        gameObject.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration = confirmButtonLegendData;

        // We don't want servers that are discovered automatically to be deleted
        if (isReadOnly)
        {
            Destroy(transform.Find("Delete").gameObject);
            return;
        }

        GameObject delete = Instantiate(deleteButtonRef, loadTransform, false);
        Button deleteButtonButton = delete.GetComponent<Button>();
        deleteButtonButton.onClick = new Button.ButtonClickedEvent();
        deleteButtonButton.onClick.AddListener(RequestDelete);

        Transform deleteTransform = this.RequireTransform("Delete");
        Destroy(deleteTransform.GetComponent<MainMenuDeleteGame>());
        Destroy(deleteTransform.GetComponent<TranslationLiveUpdate>());
        deleteCg = deleteTransform.GetComponent<CanvasGroup>();
        cancelDeleteButton = deleteTransform.RequireTransform("DeleteCancelButton").GetComponent<Button>();
        cancelDeleteButton.onClick = new Button.ButtonClickedEvent();
        cancelDeleteButton.onClick.AddListener(CancelDelete);
        Button confirmDeleteButton = deleteTransform.RequireTransform("DeleteConfirmButton").GetComponent<Button>();
        confirmDeleteButton.onClick = new Button.ButtonClickedEvent();
        confirmDeleteButton.onClick.AddListener(Delete);

        deleteTransform.gameObject.AddComponent<MainMenuDeleteServer>().serverButton = this;
        TextMeshProUGUI warningTmp = deleteTransform.RequireTransform("DeleteWarningText").GetComponent<TextMeshProUGUI>();
        warningTmp.text = Language.main.Get("Nitrox_ServerEntry_DeleteWarning");
    }

    public void RequestDelete()
    {
        uGUI_MainMenu.main.OnRightSideOpened(deleteCg.gameObject);
        uGUI_LegendBar.ClearButtons();
        uGUI_LegendBar.ChangeButton(0, uGUI.FormatButton(GameInput.Button.UICancel, gamePadOnly: true), Language.main.GetFormat("Back"));
        uGUI_LegendBar.ChangeButton(1, uGUI.FormatButton(GameInput.Button.UISubmit, gamePadOnly: true), Language.main.GetFormat("ItemSelectorSelect"));
        StartCoroutine(loadButtonRef.ShiftAlpha(loadCg, 0.0f, loadButtonRef.animTime, loadButtonRef.alphaPower, false));
        StartCoroutine(loadButtonRef.ShiftAlpha(deleteCg, 1f, loadButtonRef.animTime, loadButtonRef.alphaPower, true, cancelDeleteButton));
        StartCoroutine(loadButtonRef.ShiftPos(loadCg, MainMenuLoadButton.target.left, MainMenuLoadButton.target.centre, loadButtonRef.animTime, loadButtonRef.posPower));
        StartCoroutine(loadButtonRef.ShiftPos(deleteCg, MainMenuLoadButton.target.centre, MainMenuLoadButton.target.right, loadButtonRef.animTime, loadButtonRef.posPower));
    }

    public void CancelDelete()
    {
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
        if (GameInput.IsPrimaryDeviceGamepad())
            MainMenuServerListPanel.Main.SelectItemByIndex(MainMenuServerListPanel.Main.GetSelectedIndex());
        StartCoroutine(loadButtonRef.ShiftAlpha(loadCg, 1f, loadButtonRef.animTime, loadButtonRef.alphaPower, true));
        StartCoroutine(loadButtonRef.ShiftAlpha(deleteCg, 0.0f, loadButtonRef.animTime, loadButtonRef.alphaPower, false));
        StartCoroutine(loadButtonRef.ShiftPos(loadCg, MainMenuLoadButton.target.centre, MainMenuLoadButton.target.left, loadButtonRef.animTime, loadButtonRef.posPower));
        StartCoroutine(loadButtonRef.ShiftPos(deleteCg, MainMenuLoadButton.target.right, MainMenuLoadButton.target.centre, loadButtonRef.animTime, loadButtonRef.posPower));
    }

    public void ResetLoadDeleteView()
    {
        loadCg.alpha = 1;
        loadCg.interactable = loadCg.blocksRaycasts = true;

        RectTransform loadTransform = loadCg.GetComponent<RectTransform>();
        float loadPosX = loadTransform.sizeDelta.x * 0.5f;
        loadTransform.localPosition = new Vector3(loadPosX, loadTransform.localPosition.y, 0);

        if (deleteCg) // Read only server entries
        {
            RectTransform deleteTransform = deleteCg.GetComponent<RectTransform>();
            float deletePosX = deleteTransform.sizeDelta.x * 0.5f;
            deleteTransform.localPosition = new Vector3(deletePosX, deleteTransform.localPosition.y, 0);

            deleteCg.alpha = 0;
            deleteCg.interactable = deleteCg.blocksRaycasts = false;
        }
    }

    public void Delete()
    {
        MainMenuRightSide.main.OpenGroup(MainMenuServerListPanel.NAME);
        int scrollIndex = MainMenuServerListPanel.Main.GetSelectedIndex();
        if (GameInput.IsPrimaryDeviceGamepad() && !MainMenuServerListPanel.Main.SelectItemInYDirection(scrollIndex, 1))
        {
            MainMenuServerListPanel.Main.SelectItemInYDirection(scrollIndex, -1);
        }
        StartCoroutine(loadButtonRef.ShiftPos(deleteCg, MainMenuLoadButton.target.left, MainMenuLoadButton.target.centre, loadButtonRef.animTime, loadButtonRef.posPower));
        StartCoroutine(loadButtonRef.ShiftAlpha(deleteCg, 0.0f, loadButtonRef.animTime, loadButtonRef.alphaPower, false));

        ServerList.Instance.RemoveAt(transform.GetSiblingIndex() - 1);
        ServerList.Instance.Save();

        Destroy(gameObject);
    }

    public async Task OnJoinButtonClicked()
    {
        if (MainMenuServerListPanel.Main.IsJoining)
        {
            return; // Do not attempt to join multiple servers.
        }

        MainMenuServerListPanel.Main.IsJoining = true;
        MainMenuServerListPanel.Main.DeselectAllItems();
        await OpenJoinServerMenuAsync(joinIp, joinPort).ContinueWith(_ => { MainMenuServerListPanel.Main.IsJoining = false; });
        MainMenuJoinServerPanel.Instance.UpdatePanelValues(joinServerName);
    }

    public static async Task OpenJoinServerMenuAsync(string serverIp, int serverPort)
    {
        if (!MainMenuServerListPanel.Main)
        {
            Log.Error("MainMenuServerListPanel is not instantiated although OpenJoinServerMenuAsync is called.");
            return;
        }

        IPEndPoint endpoint = ResolveIPEndPoint(serverIp, serverPort);
        if (endpoint == null)
        {
            Log.InGame($"{Language.main.Get("Nitrox_UnableToConnect")}: {serverIp}:{serverPort}");
            return;
        }

        MainMenuNotificationPanel.ShowLoading();
        await JoinServerBackend.StartMultiplayerClientAsync(endpoint.Address, endpoint.Port);
    }

    private static IPEndPoint ResolveIPEndPoint(string serverIp, int serverPort)
    {
        UriHostNameType hostType = Uri.CheckHostName(serverIp);
        IPAddress address;
        switch (hostType)
        {
            case UriHostNameType.IPv4:
            case UriHostNameType.IPv6:
                IPAddress.TryParse(serverIp, out address);
                break;
            case UriHostNameType.Dns:
                address = ResolveHostName(serverIp, serverPort);
                break;
            default:
                return null;
        }

        return address != null ? new IPEndPoint(address, serverPort) : null;

        static IPAddress ResolveHostName(string hostname, int serverPort)
        {
            try
            {
                IPHostEntry hostEntry = Dns.GetHostEntry(hostname);
                return hostEntry.AddressList[0];
            }
            catch (SocketException ex)
            {
                Log.ErrorSensitive(ex, "Unable to resolve the address {hostname}:{serverPort}", hostname, serverPort);
                return null;
            }
        }
    }
}
