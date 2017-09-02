using Harmony;
using NitroxServer;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Server
{
    public class MainMenuPlayerName_Start_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(MainMenuPlayerName);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);

        public static ConsoleWindow consoleWindow;

        public static void Postfix(StartScreen __instance)
        {
            consoleWindow = new ConsoleWindow();
            
            string[] possibleSlotNames = SaveLoadManager.main.GetPossibleSlotNames();
            GameMode[] possibleSlotGameModes = SaveLoadManager.main.GetPossibleSlotGameModes();
            
            GameObject loadButton = new GameObject();

            MainMenuLoadButton button = loadButton.AddComponent<MainMenuLoadButton>();
            button.saveGame = possibleSlotNames[0];
            button.gameMode = possibleSlotGameModes[0];
            button.Load();

            NitroxServer.Server server = new NitroxServer.Server();
            server.Start();
        }
        
        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
