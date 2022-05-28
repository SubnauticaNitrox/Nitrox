using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.GameLogic.HUD.Components;

public class uGUI_PlayerEntry : uGUI_PingEntry
{
    private INitroxPlayer player;
    public string PlayerName => player?.PlayerName ?? string.Empty;
    public bool IsLocalPlayer => player is LocalPlayer;
    private bool showPing;
    private bool muted;

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
        Language.main.OnLanguageChanged += OnLanguageOnLanguageChanged;
    }

    public void Initialize(int id, string name)
    {
        gameObject.SetActive(true);
        this.id = id;
        visibility.isOn = true;
        visibilityIcon.sprite = spriteVisible;
        icon.sprite = SpriteManager.Get(SpriteManager.Group.Tab, "TabInventory");
        showPing = true;
        muted = false;

        UpdateLabel(name);
        OnLanguageOnLanguageChanged();
        if (AssetsHelper.AssetBundleLoaded)
        {
            AssignSprites();
        }
        else
        {
            AssetsHelper.onPlayerListAssetsLoaded += AssignSprites;
        }
    }

    public void OnLanguageOnLanguageChanged()
    {
        ShowObject.GetComponent<ButtonTooltip>().TooltipText = showPing ? Language.main.Get("Nitrox_HidePing") : Language.main.Get("Nitrox_ShowPing");
        MuteObject.GetComponent<ButtonTooltip>().TooltipText = muted ? Language.main.Get("Nitrox_Unmute") : Language.main.Get("Nitrox_Mute");
        KickObject.GetComponent<ButtonTooltip>().TooltipText = Language.main.Get("Nitrox_Kick");
        TeleportToObject.GetComponent<ButtonTooltip>().TooltipText = Language.main.Get("Nitrox_TeleportTo");
        TeleportToMeObject.GetComponent<ButtonTooltip>().TooltipText = Language.main.Get("Nitrox_TeleportToMe");
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

    public void UpdateEntryForNewPlayer(INitroxPlayer newPlayer, LocalPlayer localPlayer, IPacketSender packetSender, PlayerChatManager playerChatManager)
    {
        player = newPlayer;

        UpdateLabel(player.PlayerName);
        icon.color = player.PlayerSettings.PlayerColor.ToUnity();

        // We need to update each button's listener wether or not they have enough perms because they may become OP during playtime
        ClearButtonListeners();
        ShowObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool toggled)
        {
            if (player is RemotePlayer remotePlayer)
            {
                PingInstance pingInstance = remotePlayer.PlayerModel.GetComponentInChildren<PingInstance>();
                pingInstance.SetVisible(toggled);
                ShowObject.GetComponent<ButtonTooltip>().TooltipText = toggled ? Language.main.Get("Nitrox_HidePing") : Language.main.Get("Nitrox_ShowPing");
                visibilityIcon.sprite = toggled ? spriteVisible : spriteHidden;
            }
        });
        MuteObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool toggled)
        {
            if (player is RemotePlayer remotePlayer)
            {
                MuteObject.GetComponent<ButtonTooltip>().TooltipText = toggled ? Language.main.Get("Nitrox_Unmute") : Language.main.Get("Nitrox_Mute");
                // TODO: pass this to the MutePlayer packet processor when complement PR is merged
                MuteObject.FindChild("Eye").GetComponent<Image>().sprite = toggled ? MutedSprite : UnmutedSprite;
                string text = (toggled ? Language.main.Get("Nitrox_MutedPlayer") : Language.main.Get("Nitrox_UnmutedPlayer")).Replace("{PLAYER}", player.PlayerName);
                packetSender.Send(new ServerCommand($"{(toggled ? "" : "un")}mute {player.PlayerName}"));
            }
        });
        KickObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool toggled)
        {
            packetSender.Send(new ServerCommand($"kick {player.PlayerName}"));
        });
        TeleportToObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool toggled)
        {
            packetSender.Send(new ServerCommand($"warp {player.PlayerName}"));
        });
        TeleportToMeObject.GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool toggled)
        {
            packetSender.Send(new ServerCommand($"warp {player.PlayerName} {localPlayer.PlayerName}"));
        });

        Log.Debug($"Current perms: {localPlayer.Permissions}, {localPlayer.Permissions >= Perms.MODERATOR}");
        // We don't want none of these buttons to appear for us
        bool isNotLocalPlayer = player is not LocalPlayer;
        ShowObject.SetActive(isNotLocalPlayer);
        // The perms here should be the same as the perm each command asks for
        MuteObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        KickObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        TeleportToObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
        TeleportToMeObject.SetActive(isNotLocalPlayer && localPlayer.Permissions >= Perms.MODERATOR);
    }

    public void UpdateButtonsPosition()
    {
        MuteObject.transform.localPosition = new(
            0f,
            MuteObject.transform.localPosition.y,
            MuteObject.transform.localPosition.z);
        KickObject.transform.localPosition = new(
            80f,
            KickObject.transform.localPosition.y,
            KickObject.transform.localPosition.z);
        TeleportToObject.transform.localPosition = new(
            160f,
            TeleportToObject.transform.localPosition.y,
            TeleportToObject.transform.localPosition.z);
        TeleportToMeObject.transform.localPosition = new(
            240f,
            TeleportToMeObject.transform.localPosition.y,
            TeleportToMeObject.transform.localPosition.z);
    }

    private void ClearButtonListeners()
    {
        MuteObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        KickObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        TeleportToObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
        TeleportToMeObject.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
    }

    public void AssignSprites()
    {
        MutedSprite = AssetsHelper.MakeSpriteFromTexture("muted@2x");
        UnmutedSprite = AssetsHelper.MakeSpriteFromTexture("unmuted@2x");
        KickSprite = AssetsHelper.MakeSpriteFromTexture("kick@68x61");
        TeleportToSprite = AssetsHelper.MakeSpriteFromTexture("teleport_to@2x");
        TeleportToMeSprite = AssetsHelper.MakeSpriteFromTexture("teleport_to_me@2x");

        // TODO: keep looking for why the images don't fit the boxes
        Sprite oldSprite = MuteObject.FindChild("Eye").GetComponent<Image>().sprite;
        Log.Debug($"Mute: pixelsPerUnit: {oldSprite.pixelsPerUnit}, border: {oldSprite.border}, pivot: {oldSprite.pivot}, rect: {oldSprite.rect}, bounds: {oldSprite.bounds}, WxH: {oldSprite.texture.width}x{oldSprite.texture.height}");
        oldSprite = KickObject.FindChild("Eye").GetComponent<Image>().sprite;
        Log.Debug($"Kick: pixelsPerUnit: {oldSprite.pixelsPerUnit}, border: {oldSprite.border}, pivot: {oldSprite.pivot}, rect: {oldSprite.rect}, bounds: {oldSprite.bounds}, WxH: {oldSprite.texture.width}x{oldSprite.texture.height}");
        oldSprite = KickSprite;
        Log.Debug($"Kick: pixelsPerUnit: {oldSprite.pixelsPerUnit}, border: {oldSprite.border}, pivot: {oldSprite.pivot}, rect: {oldSprite.rect}, bounds: {oldSprite.bounds}, WxH: {oldSprite.texture.width}x{oldSprite.texture.height}");
        oldSprite = TeleportToSprite;
        Log.Debug($"Kick: pixelsPerUnit: {oldSprite.pixelsPerUnit}, border: {oldSprite.border}, pivot: {oldSprite.pivot}, rect: {oldSprite.rect}, bounds: {oldSprite.bounds}, WxH: {oldSprite.texture.width}x{oldSprite.texture.height}");

        MuteObject.FindChild("Eye").GetComponent<Image>().sprite = UnmutedSprite;
        KickObject.FindChild("Eye").GetComponent<Image>().sprite = KickSprite;
        TeleportToObject.FindChild("Eye").GetComponent<Image>().sprite = TeleportToSprite;
        TeleportToMeObject.FindChild("Eye").GetComponent<Image>().sprite = TeleportToMeSprite;
    }
}
