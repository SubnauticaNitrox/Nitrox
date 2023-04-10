using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.HUD.Components;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxModel.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static NitroxClient.Unity.Helper.AssetBundleLoader;

namespace NitroxClient.GameLogic.HUD.PdaTabs;

/// <summary>
/// The component containing a new PDA tab based on ping manager tab
/// </summary>
public class uGUI_PlayerListTab : uGUI_PingTab
{
    private NitroxPDATabManager nitroxPDATabManager;
    private PlayerManager playerManager;
    private LocalPlayer localPlayer;
    private IPacketSender packetSender;

    private readonly Dictionary<string, Sprite> assets = new();
    public bool FinishedLoadingAssets { get; private set; }

    private new readonly Dictionary<string, uGUI_PlayerPingEntry> entries = new();
    private PrefabPool<uGUI_PlayerPingEntry> pool;
    private new readonly Dictionary<string, uGUI_PlayerPingEntry> tempSort = new();

    public override void Awake()
    {
        // Copied from uGUI_PingTab.Awake but we don't want it to be executed because it creates a PrefabPool
        selectableVisibilityToggle = new SelectableWrapper(visibilityToggle, delegate (GameInput.Button button)
        {
            if (button == GameInput.Button.UISubmit)
            {
                visibilityToggle.isOn = !visibilityToggle.isOn;
                return true;
            }

            return false;
        });

        nitroxPDATabManager = NitroxServiceLocator.LocateService<NitroxPDATabManager>();
        playerManager = NitroxServiceLocator.LocateService<PlayerManager>();
        localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
        packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
        // Need to reassign manually these variables and get rid of the objects we don't need
        content = gameObject.FindChild("Content").GetComponent<CanvasGroup>();
        pingManagerLabel = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        scrollRect = gameObject.GetComponentInChildren<ScrollRect>();
        pingCanvas = (RectTransform)content.transform.Find("ScrollView/Viewport/ScrollCanvas");

        pool = new PrefabPool<uGUI_PlayerPingEntry>(prefabEntry, pingCanvas, 8, 4, delegate (uGUI_PlayerPingEntry entry)
        {
            entry.Uninitialize();
        }, delegate (uGUI_PlayerPingEntry entry)
        {
            entry.Uninitialize();
        });
    }

    public IEnumerator Start()
    {
        Transform buttonAll = content.transform.Find("ButtonAll");
        DestroyImmediate(buttonAll.gameObject);

        yield return LoadAllAssets(NitroxAssetBundle.PLAYER_LIST_TAB);

        foreach (Object asset in NitroxAssetBundle.PLAYER_LIST_TAB.LoadedAssets)
        {
            if (asset is Sprite sprite)
            {
                if (asset.name.Equals("player_list_tab@3x"))
                {
                    nitroxPDATabManager.AddTabSprite(asset.name, new Atlas.Sprite(sprite));
                }
                assets.Add(asset.name, sprite);
            }
        }

        FinishedLoadingAssets = true;
        _isDirty = true;
    }
    
    public Sprite GetSprite(string assetName)
    {
        if (assets.TryGetValue(assetName, out Sprite sprite))
        {
            return sprite;
        }
        return Sprite.Create(new Texture2D(100, 100), new Rect(0, 0, 100, 100), new Vector2(50, 50), 100);
    }

    public new void OnEnable()
    {
        // Enter events for player join and disconnect
        playerManager.onCreate += OnAdd;
        playerManager.onRemove += OnRemove;
    }

    public new void OnDestroy()
    {
        playerManager.onCreate -= OnAdd;
        playerManager.onRemove -= OnRemove;
    }

    public override void OnLanguageChanged()
    {
        pingManagerLabel.text = Language.main.Get("Nitrox_PlayerListTabName");
        entries.Values.ForEach(entry => entry.OnLanguageChanged());
    }

    public override void OnLateUpdate(bool _)
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

        Dictionary<string, INitroxPlayer> players = playerManager.GetAll().ToDictionary<RemotePlayer, string, INitroxPlayer>(player => player.PlayerId.ToString(), player => player);
        players.Add(localPlayer.PlayerId.ToString(), localPlayer);

        foreach (KeyValuePair<string, INitroxPlayer> entry in players)
        {
            if (!entries.ContainsKey(entry.Key))
            {
                // Sets up a new entry for the player
                AddNewEntry(entry.Key, entry.Value);
            }
        }

        // Sort the items by alphabetical order (based on SN's code)
        tempSort.Clear();
        foreach (KeyValuePair<string, uGUI_PlayerPingEntry> entry in entries)
        {
            if (!entry.Value.IsLocalPlayer)
            {
                tempSort.Add(entry.Value.PlayerName, entry.Value);
            }
        }

        List<string> sorted = new(tempSort.Keys);
        sorted.Sort();

        entries[localPlayer.PlayerId.ToString()].rectTransform.SetSiblingIndex(0);
        for (int j = 0; j < sorted.Count; j++)
        {
            string id = tempSort[sorted[j]].id;
            entries[id].rectTransform.SetSiblingIndex(j + 1);
        }
    }

    public uGUI_PlayerPingEntry GetEntry()
    {
        uGUI_PlayerPingEntry uGUI_PlayerEntry;
        if (pool.pool.Count == 0)
        {
            for (int i = 0; i < 4; i++)
            {
                uGUI_PlayerEntry = Instantiate(prefabEntry).GetComponent<uGUI_PlayerPingEntry>();
                uGUI_PlayerEntry.rectTransform.SetParent(pingCanvas, false);
                uGUI_PlayerEntry.Uninitialize();
                pool.pool.Add(uGUI_PlayerEntry);
            }
        }
        int index = pool.pool.Count - 1;
        uGUI_PlayerEntry = pool.pool[index];
        pool.pool.RemoveAt(index);
        return uGUI_PlayerEntry;
    }

    public void MakePrefab(GameObject basePrefab)
    {
        // We need to instantiate the prefab as we cannot directly make modifications in it
        GameObject newPrefab = Instantiate(basePrefab);
        newPrefab.name = "PlayerEntry";
        // We never want this to appear
        DestroyImmediate(newPrefab.FindChild("ColorToggle"));

        // Need to modify the pingTab's script from uGUI_PingEntry to uGUI_PlayerEntry
        uGUI_PingEntry pingEntry = newPrefab.GetComponent<uGUI_PingEntry>();
        uGUI_PlayerPingEntry playerEntry = newPrefab.AddComponent<uGUI_PlayerPingEntry>();
        playerEntry.visibility = pingEntry.visibility;
        playerEntry.visibilityIcon = pingEntry.visibilityIcon;
        playerEntry.icon = pingEntry.icon;
        playerEntry.label = pingEntry.label;
        playerEntry._rectTransform = pingEntry._rectTransform;
        playerEntry.id = pingEntry.id;
        playerEntry.spriteVisible = pingEntry.spriteVisible;
        playerEntry.spriteHidden = pingEntry.spriteHidden;
        DestroyImmediate(pingEntry);

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

    private void AddNewEntry(string playerId, INitroxPlayer player)
    {
        uGUI_PlayerPingEntry entry = GetEntry();
        entry.Initialize(playerId, player.PlayerName, this);
        entry.UpdateEntryForNewPlayer(player, localPlayer, packetSender);
        entries.Add(playerId, entry);
    }

    private void OnAdd(string playerId, RemotePlayer remotePlayer)
    {
        _isDirty = true;
    }

    private void OnRemove(string playerId, RemotePlayer remotePlayers)
    {
        if (!entries.ContainsKey(playerId))
        {
            return;
        }
        uGUI_PlayerPingEntry entry = entries[playerId];
        entries.Remove(playerId);
        pool.Release(entry);
        _isDirty = true;
    }
}
