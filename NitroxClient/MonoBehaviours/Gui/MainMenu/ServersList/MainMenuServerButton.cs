using System;
using System.Net;
using System.Net.Sockets;
using NitroxClient.MonoBehaviours.Gui.MainMenu.ServerJoin;
using NitroxClient.Unity.Helper;
using NitroxModel.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;

public class MainMenuServerButton : MonoBehaviour
{
    private static LegendButtonData[] confirmButtonLegendData;
    private static GameObject deleteButtonRef;

    private string joinIp;
    private int joinPort;

    public static void Setup(MainMenuLoadButton loadButtonRef)
    {
        confirmButtonLegendData = loadButtonRef.GetComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration;
        deleteButtonRef = loadButtonRef.deleteButton;
    }

    public void Init(string text, string ip, int port, bool isReadOnly)
    {
        joinIp = ip;
        joinPort = port;

        Transform newGameButtonTransform = this.RequireTransform("NewGameButton");

        TextMeshProUGUI tmp = newGameButtonTransform.RequireTransform("Text").GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        Destroy(tmp.GetComponent<TranslationLiveUpdate>());

        Button multiplayerJoinButton = newGameButtonTransform.GetComponent<Button>();
        multiplayerJoinButton.onClick = new Button.ButtonClickedEvent();
        multiplayerJoinButton.onClick.AddListener(OnJoinButtonClicked);

        gameObject.AddComponent<mGUI_Change_Legend_On_Select>().legendButtonConfiguration = confirmButtonLegendData;

        // We don't want servers that are discovered automatically to be deleted
        if (!isReadOnly)
        {
            GameObject delete = Instantiate(deleteButtonRef, transform, false);
            Button deleteButtonButton = delete.GetComponent<Button>();
            deleteButtonButton.onClick = new Button.ButtonClickedEvent();
            deleteButtonButton.onClick.AddListener(RequestDelete);
        }
    }

    public void RequestDelete()
    {
        ServerList.Instance.RemoveAt(transform.GetSiblingIndex() - 1);
        ServerList.Instance.Save();

        int scrollIndex = MainMenuServerListPanel.Main.GetSelectedIndex();
        if (GameInput.IsPrimaryDeviceGamepad() && !MainMenuServerListPanel.Main.SelectItemInYDirection(scrollIndex, 1))
        {
            MainMenuServerListPanel.Main.SelectItemInYDirection(scrollIndex, -1);
        }

        Destroy(gameObject);
    }

    public async void OnJoinButtonClicked()
    {
        if (MainMenuServerListPanel.Main.IsJoining)
        {
            return; // Do not attempt to join multiple servers.
        }

        MainMenuServerListPanel.Main.IsJoining = true;

        await OpenJoinServerMenuAsync(joinIp, joinPort).ContinueWith(_ => { MainMenuServerListPanel.Main.IsJoining = false; });
    }

    public static async System.Threading.Tasks.Task OpenJoinServerMenuAsync(string serverIp, int serverPort)
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
        await JoinServerBackend.Instance.ShowAsync(endpoint.Address.ToString(), endpoint.Port);
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
