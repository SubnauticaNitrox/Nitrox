using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.GameLogic.PlayerModel.ColorSwap;
using Nitrox.Client.GameLogic.PlayerModel.Equipment;
using Nitrox.Client.GameLogic.PlayerModel.Equipment.Abstract;
using Nitrox.Client.MonoBehaviours;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Nitrox.Client.GameLogic.PlayerModel
{
    public class PlayerModelManager
    {
        private readonly IEnumerable<IColorSwapManager> colorSwapManagers;
        private readonly Lazy<GameObject> signalBasePrototype;

        private GameObject SignalBasePrototype => signalBasePrototype.Value;

        public PlayerModelManager(IEnumerable<IColorSwapManager> colorSwapManagers)
        {
            this.colorSwapManagers = colorSwapManagers;
            signalBasePrototype = new Lazy<GameObject>(() =>
            {
                GameObject go = (GameObject)Object.Instantiate(Resources.Load("VFX/xSignal"));
                go.transform.localScale = new Vector3(.5f, .5f, .5f);
                go.transform.localPosition += new Vector3(0, 0.8f, 0);
                return go;
            });
        }

        public void BeginApplyPlayerColor(INitroxPlayer player)
        {
            Multiplayer.Main.StartCoroutine(ApplyPlayerColor(player, colorSwapManagers));
        }

        public void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            IEquipmentVisibilityHandler handler = BuildVisibilityHandlerChain();
            handler.UpdateEquipmentVisibility(playerModel, currentEquipment);
        }

        public void AttachPing(INitroxPlayer player)
        {
            GameObject signalBase = Object.Instantiate(SignalBasePrototype, player.PlayerModel.transform, false);
            signalBase.name = "signal" + player.PlayerName;

            PingInstance ping = signalBase.GetComponent<PingInstance>();
            ping.SetLabel("Player " + player.PlayerName);
            ping.pingType = PingType.Signal;

            UpdateLocalPlayerPda(player, ping);
            SetInGamePingColor(player, ping);
        }

        private static EquipmentVisibilityHandler BuildVisibilityHandlerChain()
        {
            return new DiveSuitVisibilityHandler()
                .WithPredecessorHandler(new ScubaSuitVisibiliyHandler())
                .WithPredecessorHandler(new FinsVisibilityHandler())
                .WithPredecessorHandler(new RadiationSuitVisibilityHandler())
                .WithPredecessorHandler(new ReinforcedSuitVisibilityHandler())
                .WithPredecessorHandler(new StillSuitVisibilityHandler());
        }

        private static void UpdateLocalPlayerPda(INitroxPlayer player, PingInstance ping)
        {
            PDA localPlayerPda = Player.main.GetPDA();
            GameObject pdaScreenGameObject = localPlayerPda.ui.gameObject;
            GameObject pingTabGameObject = pdaScreenGameObject.transform.Find("Content/PingManagerTab").gameObject;
            uGUI_PingTab pingTab = pingTabGameObject.GetComponent<uGUI_PingTab>();

            MethodInfo updateEntities = typeof(uGUI_PingTab).GetMethod("UpdateEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            updateEntities.Invoke(pingTab,
                new object[]
                {
                });

            FieldInfo pingTabEntriesField = typeof(uGUI_PingTab).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<int, uGUI_PingEntry> pingEntries = (Dictionary<int, uGUI_PingEntry>)pingTabEntriesField.GetValue(pingTab);
            uGUI_PingEntry pingEntry = pingEntries[ping.GetInstanceID()];
            pingEntry.icon.color = player.PlayerSettings.PlayerColor.ToUnity();

            GameObject pingEntryGameObject = pingEntry.gameObject;
            pingEntryGameObject.transform.Find("ColorToggle").gameObject.SetActive(false);

            if (!localPlayerPda.isInUse)
            {
                pdaScreenGameObject.gameObject.SetActive(false);
            }
        }

        private static void SetInGamePingColor(INitroxPlayer player, PingInstance ping)
        {
            uGUI_Pings pings = Object.FindObjectOfType<uGUI_Pings>();

            MethodInfo setColor = typeof(uGUI_Pings).GetMethod("OnColor", BindingFlags.NonPublic | BindingFlags.Instance);
            setColor.Invoke(pings,
                new object[]
                {
                    ping.GetInstanceID(), player.PlayerSettings.PlayerColor.ToUnity()
                });
        }

        private static IEnumerator ApplyPlayerColor(INitroxPlayer player, IEnumerable<IColorSwapManager> colorSwapManagers)
        {
            ColorSwapAsyncOperation swapOperation = new ColorSwapAsyncOperation(player, colorSwapManagers);
            swapOperation.BeginColorSwap();

            while (!swapOperation.IsColorSwapComplete())
            {
                yield return new WaitForSeconds(0.1f);
            }

            swapOperation.ApplySwappedColors();
        }
    }
}
