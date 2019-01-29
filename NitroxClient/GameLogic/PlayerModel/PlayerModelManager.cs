using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerModel.Equipment;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel
{
    public class PlayerModelManager
    {
        private readonly GameObject signalBasePrototype;

        public PlayerModelManager()
        {
            signalBasePrototype = (GameObject)Object.Instantiate(Resources.Load("VFX/xSignal"));
            signalBasePrototype.transform.localScale = new Vector3(.5f, .5f, .5f);
            signalBasePrototype.transform.localPosition += new Vector3(0, 0.8f, 0);
        }

        public void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            IEquipmentVisibilityHandler handler = BuildVisibilityHandlerChain();
            handler.UpdateEquipmentVisibility(playerModel, currentEquipment);
        }

        public void AttachPing(INitroxPlayer player)
        {
            GameObject signalBase = Object.Instantiate(signalBasePrototype);
            signalBase.name = "signal" + player.PlayerName;
            signalBase.transform.SetParent(player.PlayerModel.transform, false);

            PingInstance ping = signalBase.GetComponent<PingInstance>();
            ping.SetLabel("Player " + player.PlayerName);
            ping.pingType = PingType.Signal;

            UpdateLocalPlayerPda(player, ping);
            SetInGamePingColor(player, ping);
        }

        private EquipmentVisibilityHandler BuildVisibilityHandlerChain()
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
            updateEntities.Invoke(pingTab, new object[] { });

            FieldInfo pingTabEntriesField = typeof(uGUI_PingTab).GetField("entries", BindingFlags.NonPublic | BindingFlags.Instance);
            Dictionary<int, uGUI_PingEntry> pingEntries = (Dictionary<int, uGUI_PingEntry>)pingTabEntriesField.GetValue(pingTab);
            uGUI_PingEntry pingEntry = pingEntries[ping.GetInstanceID()];
            pingEntry.icon.color = player.PlayerSettings.PlayerColor;

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
            setColor.Invoke(pings, new object[] {ping.GetInstanceID(), player.PlayerSettings.PlayerColor});
        }
    }
}
