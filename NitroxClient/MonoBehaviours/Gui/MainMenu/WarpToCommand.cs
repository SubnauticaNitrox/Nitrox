using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class WarpToCommand : MonoBehaviour
    {
        public void Awake()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            DontDestroyOnLoad(gameObject);
        }

        public void OnDestroy()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        public void OnConsoleCommand_warpto(NotificationCenter.Notification n) // Shouldnt this be handled on the server?
        {
            if (n?.data?.Count > 0)
            {
                string otherPlayerName = (string)n.data[0];
                PlayerManager remotePlayerManager = NitroxServiceLocator.LocateService<PlayerManager>();
                Optional<RemotePlayer> opPlayer = remotePlayerManager.FindByName(otherPlayerName);
                if (opPlayer.HasValue)
                {
                    Player.main.SetPosition(opPlayer.Value.Body.transform.position);
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
