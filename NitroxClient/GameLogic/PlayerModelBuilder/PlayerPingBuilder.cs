using System;
using System.Reflection;
using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class PlayerPingBuilder : BasePlayerModelBuildHandler
    {
        protected override void HandleBuild(RemotePlayer player)
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
        
        private static void SetPingColor(RemotePlayer player, PingInstance ping)
        {
            FieldInfo field = typeof(PingManager).GetField("colorOptions", BindingFlags.Static | BindingFlags.Public);
            Color[] colors = PingManager.colorOptions;

            Color[] colorOptions = new Color[colors.Length + 1];
            colors.ForEach(color => colorOptions[Array.IndexOf(colors, color)] = color);
            colorOptions[colorOptions.Length - 1] = player.PlayerSettings.PlayerColor;

            //Replace the normal colorOptions with our colorOptions (has one color more with the player-color). Set the color of the ping with this. Then replace it back.
            field.SetValue(null, colorOptions);
            ping.SetColor(colorOptions.Length - 1);
        }
    }
}
