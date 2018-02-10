using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole;
using NitroxModel.NitroxConsole.Events;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class MultiplayerManager : MonoBehaviour
    {
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";
        private Multiplayer multiplayer;

        private void Awake()
        {
            CreatMultiplayer();
            NitroxConsole.Main.AddCommand(OnConsoleCommand);
        }

        private void CreatMultiplayer()
        {
            GameObject go = new GameObject("Multiplayer");
            go.SetActive(false); // disable GameObject so Multiplayer isn't enabled on AddComponent.
            
            multiplayer = go.AddComponent<Multiplayer>();
            multiplayer.enabled = false;

            go.transform.SetParent(transform);
            go.SetActive(true);
        }

        [NitroxCommand("mplayer")]
        [NitroxCommandArg("username", CommandArgInput.Type.STRING, true, "user", "u")]
        [NitroxCommandArg("ipaddress", CommandArgInput.Type.STRING, "ip", "i")]
        private void OnConsoleCommand(CommandEventArgs e)
        {
            if (Multiplayer.Main.enabled)
            {
                Log.InGame("Already connected to a server");
                return;
            }
            
            multiplayer.SetUserData(e.Get<string>("ipaddress").OrElse(DEFAULT_IP_ADDRESS), e.Get<string>("username").Get());
            multiplayer.enabled = true;
        }
    }
}
