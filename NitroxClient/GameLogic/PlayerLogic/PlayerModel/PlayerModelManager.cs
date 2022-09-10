﻿using System;
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

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel
{
    public class PlayerModelManager
    {
        private readonly IEnumerable<IColorSwapManager> colorSwapManagers;
        private readonly Lazy<GameObject> signalBasePrototype;

        private GameObject SignalBasePrototype => signalBasePrototype.Value;
        private List<IEquipmentVisibilityHandler> equipmentVisibilityHandlers;

        public PlayerModelManager(IEnumerable<IColorSwapManager> colorSwapManagers)
        {
            this.colorSwapManagers = colorSwapManagers;
            signalBasePrototype = new Lazy<GameObject>(() =>
            {
                GameObject go = (GameObject)Object.Instantiate(Resources.Load("VFX/xSignal"), Multiplayer.Main.transform);
                go.name = "RemotePlayerSignalPrototype";
                go.transform.localScale = new Vector3(.5f, .5f, .5f);
                go.transform.localPosition += new Vector3(0, 0.8f, 0);
                go.SetActive(false);
                return go;
            });
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

        public void AttachPing(INitroxPlayer player)
        {
            GameObject signalBase = Object.Instantiate(SignalBasePrototype, player.PlayerModel.transform, false);
            signalBase.name = "signal" + player.PlayerName;
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

            pings.OnColor(ping.GetInstanceID(), player.PlayerSettings.PlayerColor.ToUnity());
        }

        private static IEnumerator ApplyPlayerColor(INitroxPlayer player, IEnumerable<IColorSwapManager> colorSwapManagers)
        {
            ColorSwapAsyncOperation swapOperation = new(player, colorSwapManagers);
            swapOperation.BeginColorSwap();

            while (!swapOperation.IsColorSwapComplete())
            {
                yield return new WaitForSeconds(0.1f);
            }

            swapOperation.ApplySwappedColors();
        }
    }
}
