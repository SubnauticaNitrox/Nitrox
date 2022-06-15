using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxModel.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.GameLogic.HUD.PdaTabs;

/// <summary>
/// The component containing a new PDA tab based on ping manager tab
/// </summary>
public class uGUI_PlayerListTab : uGUI_PingTab
{
    private PlayerManager playerManager;
    private LocalPlayer localPlayer;
    private PlayerChatManager playerChatManager;
    private IPacketSender packetSender;

    private new Dictionary<int, uGUI_PlayerEntry> entries = new();
    private new List<uGUI_PlayerEntry> pool = new();
    private new Dictionary<string, uGUI_PlayerEntry> tempSort = new();

    public override void Awake()
    {
        base.Awake();
        playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
        localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
        playerChatManager = NitroxServiceLocator.LocateService<PlayerChatManager>();
        packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        // Need to reassign manually these variables and get rid of the objects we don't need
        content = gameObject.FindChild("Content");
        pingManagerLabel = content.FindChild("PingManagerLabel").GetComponent<Text>();
        pingCanvas = (RectTransform)content.transform.Find("ScrollView/Viewport/ScrollCanvas");

        Destroy(content.FindChild("ButtonAll"));
    }

    public new void OnEnable()
    {
        // Enter events for player join and disconnect
        playerManager.onCreate += OnAdd;
        playerManager.onRemove += OnRemove;
    }

    public override void OnLanguageChanged()
    {
        pingManagerLabel.text = Language.main.Get("Nitrox_PlayerListTabName");
    }

    public new void LateUpdate()
    {
        UpdateEntries();
    }

    public new void UpdateEntries()
    {
        if (!_isDirty)
        {
            return;
        }
        _isDirty = false;
        tempKeys.Clear();
        tempKeys.AddRange(entries.Keys);
        foreach (KeyValuePair<int, INitroxPlayer> entry in players)
        {
            int key = entry.Key;
            INitroxPlayer player = entry.Value;
            if (!entries.ContainsKey(key))
            {
                // Sets up a new entry for the player
                AddNewEntry(entry.Key, entry.Value);
            }
        }

        // Sort the items by alphabetical order (based on SN's code)
        tempSort.Clear();
        foreach (KeyValuePair<int, uGUI_PlayerEntry> entry in entries)
        {
            if (!entry.Value.IsLocalPlayer)
            {
                tempSort.Add(entry.Value.PlayerName, entry.Value);
            }
        }

        List<string> sorted = new(tempSort.Keys);
        sorted.Sort();

        entries[localPlayer.PlayerId].rectTransform.SetSiblingIndex(0);
        for (int j = 0; j < sorted.Count; j++)
        {
            int id = tempSort[sorted[j]].id;
            entries[id].rectTransform.SetSiblingIndex(j + 1);
        }
    }

    public new uGUI_PlayerEntry GetEntry()
    {
        uGUI_PlayerEntry uGUI_PlayerEntry;
        if (pool.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                uGUI_PlayerEntry = Instantiate(prefabEntry).GetComponent<uGUI_PlayerEntry>();
                uGUI_PlayerEntry.rectTransform.SetParent(pingCanvas, false);
                uGUI_PlayerEntry.Uninitialize();
                pool.Add(uGUI_PlayerEntry);
            }
        }
        int index = pool.Count - 1;
        uGUI_PlayerEntry = pool[index];
        pool.RemoveAt(index);
        return uGUI_PlayerEntry;
    }

    public void MakePrefab(GameObject basePrefab)
    {
        // We need to instantiate the prefab as we cannot directly make modifications in it
        GameObject newPrefab = Instantiate(basePrefab);
        newPrefab.name = "PlayerEntry";
        // We never want this to appear
        Destroy(newPrefab.FindChild("ColorToggle"));

        // Need to modify the pingTab's script from uGUI_PingEntry to uGUI_PlayerEntry
        uGUI_PingEntry pingEntry = newPrefab.GetComponent<uGUI_PingEntry>();
        uGUI_PlayerEntry playerEntry = newPrefab.AddComponent<uGUI_PlayerEntry>();
        playerEntry.visibility = pingEntry.visibility;
        playerEntry.visibilityIcon = pingEntry.visibilityIcon;
        playerEntry.icon = pingEntry.icon;
        playerEntry.label = pingEntry.label;
        playerEntry._rectTransform = pingEntry._rectTransform;
        playerEntry.id = pingEntry.id;
        playerEntry.spriteVisible = pingEntry.spriteVisible;
        playerEntry.spriteHidden = pingEntry.spriteHidden;
        Destroy(pingEntry);

        // Make buttons for mute, kick, tp
        Transform container = newPrefab.transform;
        playerEntry.ShowObject = newPrefab.FindChild("ButtonVisibility");
        playerEntry.ShowObject.AddComponent<ButtonTooltip>();

        playerEntry.MuteObject = Instantiate(playerEntry.ShowObject, container);
        playerEntry.KickObject = Instantiate(playerEntry.ShowObject, container);
        playerEntry.TeleportToObject = Instantiate(playerEntry.ShowObject, container);
        playerEntry.TeleportToMeObject = Instantiate(playerEntry.ShowObject, container);
        playerEntry.MuteObject.name = "MuteObject";
        playerEntry.KickObject.name = "KickObject";
        playerEntry.TeleportToObject.name = "TeleportToObject";
        playerEntry.TeleportToMeObject.name = "TeleportToMeObject";

        prefabEntry = newPrefab;
    }

    private void AddNewEntry(int playerId, INitroxPlayer player)
    {
        uGUI_PlayerEntry entry = GetEntry();
        entry.Initialize(playerId, player.PlayerName);
        entry.UpdateEntryForNewPlayer(player, localPlayer, packetSender, playerChatManager);
        entries.Add(playerId, entry);
    }

    private void OnAdd(ushort playerId, RemotePlayer remotePlayer)
    {
        _isDirty = true;
    }

    private void OnRemove(ushort playerId, RemotePlayer remotePlayers)
    {
        if (!entries.ContainsKey(playerId))
        {
            return;
        }
        uGUI_PingEntry entry = entries[playerId];
        entries.Remove(playerId);
        ReleaseEntry(entry);
        _isDirty = true;
    }

    private Dictionary<int, INitroxPlayer> players
    {
        get
        {
            Dictionary<int, INitroxPlayer> players = new();
            foreach (RemotePlayer player in playerManager.GetAll())
            {
                players.Add(player.PlayerId, player);
            }
            players.Add(localPlayer.PlayerId, localPlayer);
            return players;
        }
    }
}
