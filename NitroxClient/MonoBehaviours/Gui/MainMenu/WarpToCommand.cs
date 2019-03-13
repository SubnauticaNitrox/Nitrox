using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class WarpToCommand : MonoBehaviour
    {
        private const string DEFAULT_IP_ADDRESS = "127.0.0.1";

        public void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void OnConsoleCommand_warpto(NotificationCenter.Notification n)
        {
            if (n?.data?.Count > 0)
            {
                string otherPlayerName = (string)n.data[0];
                PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                Optional<RemotePlayer> opPlayer = remotePlayerManager.FindByName(otherPlayerName);
                if (opPlayer.IsPresent())
                {
                    Player.main.SetPosition(opPlayer.Get().Body.transform.position);
                    Player.main.OnPlayerPositionCheat();
                }
            }
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            DevConsole.RegisterConsoleCommand(this, "warpto", false);
        }
    }
}
