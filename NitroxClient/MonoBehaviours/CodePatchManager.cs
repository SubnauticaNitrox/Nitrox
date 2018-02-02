using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours
{
    public class CodePatchManager : MonoBehaviour
    {
        public static event EventHandler Restore;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            if (scene.name == "XMenu")
            {
                OnRestore(this, EventArgs.Empty);
            }
        }

        private void OnRestore(object sender, EventArgs e)
        {
            Restore?.Invoke(sender, e);
        }
    }
}
