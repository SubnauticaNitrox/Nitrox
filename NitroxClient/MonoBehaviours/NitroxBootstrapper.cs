using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxBootstrapper : MonoBehaviour
    {
        internal static NitroxBootstrapper Instance;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            gameObject.AddComponent<SceneCleanerPreserve>();
            gameObject.AddComponent<MainMenuMods>();

#if DEBUG
            EnableDeveloperFeatures();
#endif

            CreateDebugger();
        }

        private void Update()
        {
            static void UpdateInput()
            {
                // TODO: Use proper way to check if input is free, because players can be editing labels etc.
                if (DevConsole.instance.state)
                {
                    return;
                }
            
                foreach (KeyBindingManager.KeyBinding key in KeyBindingManager.KeyboardBindings)
                {
                    if (key.InGameOnly && GameInput.GetButtonDown(key.Button)) // GameInput logic only works while playing in the game world.
                    {
                        key.Action();
                    }
                    else if (!key.InGameOnly && Input.GetKeyDown(key.Current)) // Our input handling to make inputs also work in main menu (if required).
                    {
                        key.Action();
                    }
                }
            }
            
            UpdateInput();
        }

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
            DevConsole.disableConsole = false;
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to \"{Application.runInBackground}\"");
        }

        private void CreateDebugger()
        {
            GameObject debugger = new GameObject();
            debugger.name = "Debug manager";
            debugger.AddComponent<NitroxDebugManager>();
            debugger.transform.SetParent(transform);
        }
    }
}
