using NitroxModel.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        public string serverIp = "";
        string username = "username";
        bool showingUsername = false;
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            showingUsername = true;
        }

        public void OnGUI()
        {
            if (!showingUsername)
            {
                return;
            }
            username = GUI.TextField(new Rect(Screen.width / 2 - 250, Screen.height / 2 - 25, 500, 50), username);
            if (GUI.Button(new Rect(Screen.width / 2 - 250, Screen.height / 2 + 25, 500, 50), "Add server"))
            {
                StartCoroutine(JoinServerWait(serverIp));
                showingUsername = false;
            }
        }

        public IEnumerator JoinServerWait(string serverIp)
        {
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Creative);
            StartCoroutine(startNewGame);
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);
            Multiplayer.PacketSender.PlayerId = username;
            Multiplayer.main.StartMultiplayer(serverIp);
            Multiplayer.main.InitMonoBehaviours();
            Destroy(gameObject);
        }
    }
}
