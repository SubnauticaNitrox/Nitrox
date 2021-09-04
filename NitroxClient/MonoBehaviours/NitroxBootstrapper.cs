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

        private void EnableDeveloperFeatures()
        {
            Log.Info("Enabling developer console.");
#if SUBNAUTICA
            DevConsole.disableConsole = false;
#elif BELOWZERO
            PlatformUtils.SetDevToolsEnabled(false);
#endif
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
