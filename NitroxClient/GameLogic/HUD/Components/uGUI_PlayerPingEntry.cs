using System.Collections;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.GameLogic.HUD.PdaTabs;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace NitroxClient.GameLogic.HUD.Components;

public class uGUI_PlayerPingEntry : uGUI_PingEntry
{
    private uGUI_PlayerListTab parent;
    private INitroxPlayer player;

    public string PlayerName => player?.PlayerName ?? string.Empty;
    public bool IsLocalPlayer => player is LocalPlayer;
    private bool showPing;
    private bool _muted;
    private bool muted
    {
        get
        {
            if (player is RemotePlayer remotePlayer && remotePlayer.PlayerContext != null)
            {
                return remotePlayer.PlayerContext.IsMuted;
            }
            return _muted;
        }
    }

    public GameObject ShowObject;
    public GameObject MuteObject;
    public GameObject KickObject;
    public GameObject TeleportToObject;
    public GameObject TeleportToMeObject;

    public Sprite MutedSprite;
    public Sprite UnmutedSprite;
    public Sprite KickSprite;
    public Sprite TeleportToSprite;
    public Sprite TeleportToMeSprite;

    public void Awake()
    {
        NitroxServiceLocator.LocateService<MutePlayerProcessor>().OnPlayerMuted += (playerId, muted) =>
        {
            if ((player is RemotePlayer remotePlayer && remotePlayer.PlayerId == playerId) ||
                (player is LocalPlayer localPlayer && localPlayer.PlayerId == playerId))
            {
                RefreshMuteButton(muted);
            }
        };
        NitroxServiceLocator.LocateService<PermsChangedProcessor>().OnPermissionsChanged += (perms) => RefreshButtonsVisibility();
    }

    public void Start()
    {
        // This action must happen here, else the button will be moved
        UpdateButtonsPosition();
        // We trigger it at least once so that the localizations are updated with the PlayerName
        OnLanguageChanged();
    }

    public void Initialize(int id, string name, uGUI_PlayerListTab parent)
    {
        this.parent = parent;

        gameObject.SetActive(true);
        this.id = id;
        visibility.isOn = true;
        visibilityIcon.sprite = spriteVisible;
        icon.sprite = SpriteManager.Get(SpriteManager.Group.Tab, "TabInventory");
        showPing = true;
        _muted = false;

        UpdateLabel(name);
        OnLanguageChanged();

        CoroutineHost.StartCoroutine(AssignSprites());
    }

    public void OnLanguageChanged()
    {
        GetTooltip(ShowObject).TooltipText = Language.main.Get(showPing ? "Nitrox_HidePing" : "Nitrox_ShowPing");
        GetTooltip(MuteObject).TooltipText = Language.main.Get(muted ? "Nitrox_Unmute" : "Nitrox_Mute");
        GetTooltip(KickObject).TooltipText = Language.main.Get("Nitrox_Kick");
        GetTooltip(TeleportToObject).TooltipText = Language.main.Get("Nitrox_TeleportTo").Replace("{PLAYER}", PlayerName);
        GetTooltip(TeleportToMeObject).TooltipText = Language.main.Get("Nitrox_TeleportToMe").Replace("{PLAYER}", PlayerName);
    }

    public new void Uninitialize()
    {
        base.Uninitialize();
        player = null;
    }

    public void UpdateLabel(string text)
    {
        label.text = text;
    }

    public void UpdateEntryForNewPlayer(INitroxPlayer newPlayer, LocalPlayer localPlayer, IPacketSender packetSender)
    {
        player = newPlayer;

        UpdateLabel(player.PlayerName);
        icon.color = player.PlayerSettings.PlayerColor.ToUnity();
        if (newPlayer is RemotePlayer remotePlayer)
        {
            RefreshMuteButton(remotePlayer.PlayerContext.IsMuted);
        }

        // We need to update each button's listener wether or not they have enough perms because they may become OP during playtime
        ClearButtonListeners();

        GetToggle(ShowObject).onValueChanged.AddListener(delegate (bool toggled)
        {
            if (player is RemotePlayer remotePlayer)
            {
                PingInstance pingInstance = remotePlayer.PlayerModel.GetComponentInChildren<PingInstance>();
                pingInstance.SetVisible(toggled);
                GetTooltip(ShowObject).TooltipText = Language.main.Get(toggled ? "Nitrox_HidePing" : "Nitrox_ShowPing");
                visibilityIcon.sprite = toggled ? spriteVisible : spriteHidden;
            }
        });
        // Each of those clicks involves a confirmation modal
        GetToggle(MuteObject).onValueChanged.AddListener(delegate (bool toggled)
        {
            string text = Language.main.Get(muted ? "Nitrox_Unmute" : "Nitrox_Mute");
            Modal.Get<ConfirmModal>()?.Show($"{text} {player.PlayerName}?", () =>
            {
                GetToggle(MuteObject).SetIsOnWithoutNotify(!toggled);
                if (player is RemotePlayer remotePlayer)
                {
                    packetSender.Send(new ServerCommand($"{(toggled ? "" : "un")}mute {player.PlayerName}"));
                }
            });
        });
        GetToggle(KickObject).onValueChanged.AddListener(delegate (bool toggled)
        {
            Modal.Get<ConfirmModal>()?.Show($"{Language.main.Get("Nitrox_Kick")} {player.PlayerName}?", () =>
            {
                packetSender.Send(new ServerCommand($"kick {player.PlayerName}"));
            });
        });
        GetToggle(TeleportToObject).onValueChanged.AddListener(delegate (bool toggled)
        {
            string text = Language.main.Get("Nitrox_TeleportTo").Replace("{PLAYER}", player.PlayerName);
            Modal.Get<ConfirmModal>()?.Show($"{text}?", () =>
            {
                packetSender.Send(new ServerCommand($"warp {player.PlayerName}"));
            });
        });
        GetToggle(TeleportToMeObject).onValueChanged.AddListener(delegate (bool toggled)
        {
            string text = Language.main.Get("Nitrox_TeleportToMe").Replace("{PLAYER}", player.PlayerName);
            Modal.Get<ConfirmModal>()?.Show($"{text}?", () =>
            {
                packetSender.Send(new ServerCommand($"warp {player.PlayerName} {localPlayer.PlayerName}"));
            });
        });

        RefreshButtonsVisibility();
    }

    public void UpdateButtonsPosition()
    {
        float offset = 540f;

        MuteObject.transform.localPosition = new(
            0f + offset,
            MuteObject.transform.localPosition.y,
            MuteObject.transform.localPosition.z);
        KickObject.transform.localPosition = new(
            80f + offset,
            KickObject.transform.localPosition.y,
            KickObject.transform.localPosition.z);
        TeleportToObject.transform.localPosition = new(
            160f + offset,
            TeleportToObject.transform.localPosition.y,
            TeleportToObject.transform.localPosition.z);
        TeleportToMeObject.transform.localPosition = new(
            240f + offset,
            TeleportToMeObject.transform.localPosition.y,
            TeleportToMeObject.transform.localPosition.z);
    }

    private void ClearButtonListeners()
    {
        GetToggle(MuteObject).onValueChanged.RemoveAllListeners();
        GetToggle(KickObject).onValueChanged.RemoveAllListeners();
        GetToggle(TeleportToObject).onValueChanged.RemoveAllListeners();
        GetToggle(TeleportToMeObject).onValueChanged.RemoveAllListeners();
    }

    private IEnumerator AssignSprites()
    {
        yield return new WaitUntil(() => parent.FinishedLoadingAssets);
        
        // NB: Those textures MUST be exported with a Texture Type of "Sprite (2D and UI)", else they will look blurry not matter what
        // NB 2: Those textures for the buttons are scaled 68x61 but the image inside but not hit the borders to have a better render
        MutedSprite = parent.GetSprite("muted@3x");
        UnmutedSprite = parent.GetSprite("unmuted@3x");
        KickSprite = parent.GetSprite("kick@3x");
        TeleportToSprite = parent.GetSprite("teleport_to@3x");
        TeleportToMeSprite = parent.GetSprite("teleport_to_me@3x");
        
        MuteObject.FindChild("Eye").GetComponent<Image>().sprite = UnmutedSprite;
        KickObject.FindChild("Eye").GetComponent<Image>().sprite = KickSprite;
        TeleportToObject.FindChild("Eye").GetComponent<Image>().sprite = TeleportToSprite;
        TeleportToMeObject.FindChild("Eye").GetComponent<Image>().sprite = TeleportToMeSprite;
    }

    private void RefreshMuteButton(bool muted)
    {
        GetToggle(MuteObject).SetIsOnWithoutNotify(muted);
        GetTooltip(MuteObject).TooltipText = Language.main.Get(muted ? "Nitrox_Unmute" : "Nitrox_Mute");
        MuteObject.FindChild("Eye").GetComponent<Image>().sprite = muted ? MutedSprite : UnmutedSprite;
    }

    private void RefreshButtonsVisibility()
    {
        LocalPlayer localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
        
        bool isNotLocalPlayer = !IsLocalPlayer || true;
        // We don't want none of these buttons to appear for us
        ShowObject.SetActive(isNotLocalPlayer);

        // The perms here should be the same as the perm each command asks for
        MuteObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        KickObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        TeleportToObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        TeleportToMeObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
    }

    private Toggle GetToggle(GameObject gameObject)
    {
        return gameObject.GetComponent<Toggle>();
    }

    private ButtonTooltip GetTooltip(GameObject gameObject)
    {
        return gameObject.GetComponent<ButtonTooltip>();
    }
}
