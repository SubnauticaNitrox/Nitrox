using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole;
using NitroxModel.NitroxConsole.Events;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxBootstrapper : MonoBehaviour
    {
        public static NitroxBootstrapper Main { get; private set; }

        public void Awake()
        {
            Main = this;

            // Please don't kill me..
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<SceneCleanerPreserve>();

            // Global managers WITHOUT child objects.
            gameObject.AddComponent<NitroxConsoleUI>();
            gameObject.AddComponent<MainMenuMods>();
            gameObject.AddComponent<CodePatchManager>();

            // Global managers WITH child objects.
            CreateDebugger();
            CreateMultiplayer();

            NitroxConsole.Main.AddCommand(QuitGameCommand);
        }

        private void CreateDebugger()
        {
            GameObject debugger = new GameObject();
            debugger.name = "Debug manager";
            debugger.AddComponent<NitroxDebugManager>();
            debugger.transform.SetParent(transform);
        }

        private void CreateMultiplayer()
        {
            GameObject mp = new GameObject();
            mp.name = "Multiplayer manager";
            mp.AddComponent<MultiplayerManager>();
            mp.transform.SetParent(transform);
        }

        [NitroxCommand("quit")]
        private static void QuitGameCommand(CommandEventArgs e)
        {
            Application.Quit();
        }
    }
}
