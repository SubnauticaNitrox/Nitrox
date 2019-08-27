using NitroxClient.MonoBehaviours.Gui.MainMenu;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxBootstrapper : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<SceneCleanerPreserve>();
            gameObject.AddComponent<MainMenuMods>();

#if DEBUG
            AttachWarpToCommand();
            AttachGiveCommand();
#endif

            CreateDebugger();
        }

        private void AttachGiveCommand()
        {
            GameObject consoleRoot = new GameObject();
            consoleRoot.AddComponent<GiveCommand>();
        }

        private void AttachWarpToCommand()
        {
            GameObject consoleRoot = new GameObject();
            consoleRoot.AddComponent<WarpToCommand>();
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
