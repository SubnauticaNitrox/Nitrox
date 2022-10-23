using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel;

public class PlayerModelManager
{
    private readonly IEnumerable<IColorSwapManager> colorSwapManagers;
    private List<IEquipmentVisibilityHandler> equipmentVisibilityHandlers;

    public PlayerModelManager(IEnumerable<IColorSwapManager> colorSwapManagers)
    {
        this.colorSwapManagers = colorSwapManagers;
    }

    public void BeginApplyPlayerColor(INitroxPlayer player)
    {
        Multiplayer.Main.StartCoroutine(ApplyPlayerColor(player, colorSwapManagers));
    }

    public void RegisterEquipmentVisibilityHandler(GameObject playerModel)
    {
        equipmentVisibilityHandlers = new List<IEquipmentVisibilityHandler>
        {
            new DiveSuitVisibilityHandler(playerModel),
            new ScubaSuitVisibilityHandler(playerModel),
            new FinsVisibilityHandler(playerModel),
            new RadiationSuitVisibilityHandler(playerModel),
            new ReinforcedSuitVisibilityHandler(playerModel),
            new StillSuitVisibilityHandler(playerModel)
        };
    }

    public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
    {
        foreach (IEquipmentVisibilityHandler equipmentVisibilityHandler in equipmentVisibilityHandlers)
        {
            equipmentVisibilityHandler.UpdateEquipmentVisibility(currentEquipment);
        }
    }

    private IEnumerator CreateSignalPrototype(IOut<GameObject> result)
    {
        CoroutineTask<GameObject> signalHandle = AddressablesUtility.InstantiateAsync("WorldEntities/Environment/Generated/Signal.prefab", Multiplayer.Main.transform, awake: false);
        yield return signalHandle;

        GameObject go = signalHandle.GetResult();
        go.name = "RemotePlayerSignalPrototype";
        go.transform.localScale = new Vector3(.5f, .5f, .5f);
        go.transform.localPosition += new Vector3(0, 0.8f, 0);
        go.SetActive(false);

        result.Set(go);
    }

    public IEnumerator AttachPing(INitroxPlayer player)
    {
        TaskResult<GameObject> result = new();
        yield return CreateSignalPrototype(result);

        GameObject signalBase = Object.Instantiate(result.value, player.PlayerModel.transform, false);
        signalBase.name = $"signal_{player.PlayerName}";
        signalBase.SetActive(true);

        PingInstance ping = signalBase.GetComponent<PingInstance>();
        ping.SetLabel("Player " + player.PlayerName);
        ping.pingType = PingType.Signal;
        // ping will be moved to the player list tab
        ping.displayPingInManager = false;

        UpdateLocalPlayerPda();
        SetInGamePingColor(player, ping);
    }

    private static void UpdateLocalPlayerPda()
    {
        PDA localPlayerPda = Player.main.GetPDA();
        GameObject pdaScreenGameObject = localPlayerPda.ui.gameObject;
        GameObject pingTabGameObject = pdaScreenGameObject.transform.Find("Content/PingManagerTab").gameObject;
        uGUI_PingTab pingTab = pingTabGameObject.GetComponent<uGUI_PingTab>();

        pingTab.UpdateEntries();

        if (!localPlayerPda.isInUse)
        {
            pdaScreenGameObject.gameObject.SetActive(false);
        }
    }

    private static void SetInGamePingColor(INitroxPlayer player, PingInstance ping)
    {
        uGUI_Pings pings = Object.FindObjectOfType<uGUI_Pings>();

        pings.OnColor(ping.Id, player.PlayerSettings.PlayerColor.ToUnity());
    }

    private static IEnumerator ApplyPlayerColor(INitroxPlayer player, IEnumerable<IColorSwapManager> colorSwapManagers)
    {
        ColorSwapAsyncOperation swapOperation = new(player, colorSwapManagers);

        swapOperation.BeginColorSwap();
        yield return new WaitUntil(() => swapOperation.IsColorSwapComplete());
        swapOperation.ApplySwappedColors();
    }
}
