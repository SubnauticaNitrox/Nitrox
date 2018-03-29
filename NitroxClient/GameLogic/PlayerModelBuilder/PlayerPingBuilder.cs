using System;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class PlayerPingBuilder : IPlayerModelBuilder
    {
        public void Build(INitroxPlayer player)
        {
            GameObject signalBase = Object.Instantiate(Resources.Load("VFX/xSignal")) as GameObject;
            signalBase.name = "signal" + player.PlayerName;
            signalBase.transform.localScale = new Vector3(.5f, .5f, .5f);
            signalBase.transform.localPosition += new Vector3(0, 0.8f, 0);
            signalBase.transform.SetParent(player.PlayerModel.transform, false);

            PingInstance ping = signalBase.GetComponent<PingInstance>();
            ping.SetLabel("Player " + player.PlayerName);
            ping.pingType = PingType.Signal;

            UpdateLocalPlayerPda(player, ping);
            SetInGamePingColor(player, ping);
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
            Dictionary<int, uGUI_PingEntry> pingEntries = (Dictionary<int, uGUI_PingEntry>) pingTabEntriesField.GetValue(pingTab);
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
