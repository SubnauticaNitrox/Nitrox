using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.MonoBehaviours.Debugging
{
    public class SceneDebugger : BaseDebugger
    {
        private Scene selectedScene;
        private GameObject selectedObject;
        private Vector2 hierarchyScrollPos;
        private Vector2 gameObjectScrollPos;
        private Color paleGreen = new Color(152, 251, 152);
        private const int MAX_HIERARCHY_ENTRIES = 100;

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
                    using (new GUILayout.HorizontalScope("Box"))
                    {
                        StringBuilder breadcrumBuilder = new StringBuilder();
                        if (selectedObject != null)
                        {
                            Transform parent = selectedObject.transform;
                            while (parent != null)
                            {
                                breadcrumBuilder.Insert(0, '/');
                                breadcrumBuilder.Insert(0, string.IsNullOrEmpty(parent.name) ? "<no-name>" : parent.name);
                                parent = parent.parent;
                            }
                        }
                        breadcrumBuilder.Insert(0, "//");
                        GUILayout.Label(breadcrumBuilder.ToString(), "breadcrum");

                        using (new GUILayout.HorizontalScope("breadcumNav"))
                        {
                            if (GUILayout.Button("<<"))
                            {
                                selectedObject = null;
                            }
                            if (GUILayout.Button("<"))
                            {
                                selectedObject = selectedObject?.transform.parent?.gameObject;
                            }
                        }
                    }

                    using (new GUILayout.VerticalScope("Box"))
                    {
                        if (selectedScene.IsValid())
                        {
                            using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(hierarchyScrollPos))
                            {
                                hierarchyScrollPos = scroll.scrollPosition;
                                List<GameObject> showObjects = new List<GameObject>();
                                if (selectedObject == null)
                                {
                                    showObjects = selectedScene.GetRootGameObjects().ToList();
                                }
                                else
                                {
                                    foreach (Transform t in selectedObject.transform)
                                    {
                                        showObjects.Add(t.gameObject);
                                    }
                                }
                                foreach (GameObject child in showObjects)
                                {
                                    if (GUILayout.Button($"{child.name}", "label"))
                                    {
                                        selectedObject = child.gameObject;
                                    }
                                }
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
                            using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(gameObjectScrollPos))
                            {
                                gameObjectScrollPos = scroll.scrollPosition;

                                GUILayout.Label($"GameObject: {selectedObject.name}", "header");

                                //Add transform interface.
                                using (new GUILayout.VerticalScope("Box"))
                                {
                                    GUILayout.Label("Transform");
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        //TODO: Create a "save" button to save changes instead of realtime editing.
                                        Vector3 pos = selectedObject.transform.position;
                                        float.TryParse(GUILayout.TextField(pos.x.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.x);
                                        float.TryParse(GUILayout.TextField(pos.y.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.y);
                                        float.TryParse(GUILayout.TextField(pos.z.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.z);
                                        selectedObject.transform.position = pos;
                                    }
                                }

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

            skin.SetCustomStyle("breadcrum", skin.label, s =>
            {
                s.fontSize = 20;
                s.fontStyle = FontStyle.Bold;
            });

            skin.SetCustomStyle("breadcumNav", skin.box, s =>
            {
                s.stretchWidth = false;
                s.fixedWidth = 100;
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
