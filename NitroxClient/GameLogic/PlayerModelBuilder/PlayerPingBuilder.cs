using System;
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

            SetPingColor(player, ping);
        }

        private static void SetPingColor(INitroxPlayer player, PingInstance ping)
        {
            FieldInfo field = typeof(PingManager).GetField("colorOptions", BindingFlags.Static | BindingFlags.Public);
            Color[] originalColorOptions = PingManager.colorOptions;

            Color[] newColorOptions = new Color[originalColorOptions.Length + 1];
            originalColorOptions.ForEach(color => newColorOptions[Array.IndexOf(originalColorOptions, color)] = color);
            newColorOptions[newColorOptions.Length - 1] = player.PlayerSettings.PlayerColor;

            // Replace the normal colorOptions with our colorOptions (has one color more with the player-color). Set the color of the ping with this. Then replace it back.
            field.SetValue(null, newColorOptions);
            ping.SetColor(newColorOptions.Length - 1);
        }
    }
}
