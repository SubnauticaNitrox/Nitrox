using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class SceneDebugger : BaseDebugger
    {
        private Scene selectedScene;
        private GameObject selectedObject;
        private Vector2 hierarchyScrollPos;
        private Color paleGreen = new Color(152, 251, 152);

        public void Awake()
        {
            Hotkey = KeyCode.S;
            HotkeyControlRequired = true;
            WindowRect.width = 400;
            SkinCreationOptions = GUISkinCreationOptions.DERIVEDCOPY;

            Tabs.AddRange(new[]
            {
                "Scenes",
                "Hierarchy",
                "GameObject"
            });
        }

        public override void DoWindow(int windowId)
        {
            switch (ActiveTab)
            {
                default:
                case 0:
                    Scene scene = SceneManager.GetActiveScene();
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        GUILayout.Label("All scenes", "header");
                        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                        {
                            Scene currentScene = SceneManager.GetSceneByBuildIndex(i);
                            string path = SceneUtility.GetScenePathByBuildIndex(i);
                            bool isSelected = selectedScene.IsValid() && currentScene == selectedScene;
                            bool isLoaded = currentScene.isLoaded;

                            using (new GUILayout.HorizontalScope("Box"))
                            {
                                if (GUILayout.Button($"{(isSelected ? ">> " : "")}{i}: {path.TruncateLeft(35)}", isLoaded ? "sceneLoaded" : "label"))
                                {
                                    selectedScene = currentScene;
                                    ActiveTab = 1;
                                }
                                if (GUILayout.Button(isLoaded ? "Unload" : "Load", "loadScene"))
                                {
                                    if (isLoaded)
                                    {
                                        SceneManager.UnloadSceneAsync(i);
                                    }
                                    else
                                    {
                                        SceneManager.LoadSceneAsync(i);
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 1:
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        if (selectedScene.IsValid())
                        {
                            using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(hierarchyScrollPos))
                            {
                                hierarchyScrollPos = scroll.scrollPosition;
                                GuiRenderTree(selectedScene.GetRootGameObjects().Select(g => g.transform));
                            }
                        }
                        else
                        {
                            GUILayout.Label($"No selected scene\nClick on a Scene in '{Tabs[0]}'", "fillMessage");
                        }
                    }
                    break;

                case 2:
                    using (new GUILayout.VerticalScope("Box"))
                    {
                        if (selectedObject)
                        {
                            GUILayout.Label($"GameObject: {selectedObject.name}", "header");

                            //Add transform interface.
                            //using (new GUILayout.VerticalScope("Box"))
                            //{
                            //    GUILayout.Label("Transform");
                            //    using (new GUILayout.HorizontalScope())
                            //    {
                            //        transformX = GUILayout.TextField(transformX);
                            //        transformY = GUILayout.TextField(transformY);
                            //        transformZ = GUILayout.TextField(transformZ);
                            //        selectedObject.transform.position = new Vector3(float.Parse(transformX), float.Parse(transformY), float.Parse(transformZ));
                            //    }
                            //}

                            //Add other component interfaces.
                            foreach (MonoBehaviour behaviour in selectedObject.GetComponents<MonoBehaviour>())
                            {
                                Type script = behaviour.GetType();
                                using (new GUILayout.VerticalScope("Box"))
                                {
                                    GUILayout.Label(script.Name);
                                }
                            }
                        }
                        else
                        {
                            GUILayout.Label($"No selected GameObject\nClick on an object in '{Tabs[1]}'", "fillMessage");
                        }
                    }
                    break;
            }
        }

        protected override void OnSetSkin(GUISkin skin)
        {
            base.OnSetSkin(skin);

            skin.SetCustomStyle("sceneLoaded", skin.label, s =>
            {
                s.normal = new GUIStyleState()
                {
                    textColor = Color.green
                };
                s.fontStyle = FontStyle.Bold;
            });

            skin.SetCustomStyle("loadScene", skin.button, s =>
            {
                s.fixedWidth = 60;
            });

            skin.SetCustomStyle("mainScene", skin.label, s =>
            {
                s.margin = new RectOffset(2, 10, 10, 10);
                s.alignment = TextAnchor.MiddleLeft;
                s.fontSize = 20;
            });

            skin.SetCustomStyle("fillMessage", skin.label, s =>
            {
                s.stretchWidth = true;
                s.stretchHeight = true;
                s.fontSize = 24;
                s.alignment = TextAnchor.MiddleCenter;
                s.fontStyle = FontStyle.Italic;
            });
        }

        private void GuiRenderTree(IEnumerable<Transform> objects, int level = 0)
        {
            foreach (Transform t in objects)
            {
                if (GUILayout.Button($"{(t.gameObject == selectedObject ? ">>" : "")}{new string(' ', level * 4)}{t.name}", "label"))
                {
                    selectedObject = t.gameObject;
                    ActiveTab = 2;
                }

                List<Transform> transforms = new List<Transform>();
                foreach (Transform child in t)
                {
                    transforms.Add(child);
                }
                GuiRenderTree(transforms, level + 1);
            }
        }
    }
}
