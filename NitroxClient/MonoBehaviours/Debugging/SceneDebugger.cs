using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class SceneDebugger : BaseDebugger
    {
        private static GUIStyle sceneLoadedStyle;

        public void Awake()
        {
            if (sceneLoadedStyle == null)
            {
                sceneLoadedStyle = new GUIStyle()
                {
                    normal = new GUIStyleState()
                    {
                        textColor = Color.green
                    },
                    fontStyle = FontStyle.Bold
                };
            }

            Hotkey = KeyCode.S;
            HotkeyControlRequired = true;
            WindowRect.width = 300;
        }

        public override void DoWindow(int windowId)
        {
            Scene scene = SceneManager.GetActiveScene();
            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label("All scenes");
                for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    string path = SceneUtility.GetScenePathByBuildIndex(i);
                    bool isLoaded = SceneManager.GetSceneByBuildIndex(i).isLoaded;
                    GUILayout.Label($"{i}: {path.TruncateLeft(35)}", isLoaded ? sceneLoadedStyle : GUI.skin.label);
                }
            }

            using (new GUILayout.VerticalScope("Box"))
            {
                GUILayout.Label($"Scene: {scene.name}");
            }
        }
    }
}
