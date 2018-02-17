using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.Debuggers
{
    public class SceneDebugger : BaseDebugger
    {
        private Vector2 gameObjectScrollPos;
        private Vector2 hierarchyScrollPos;
        private GameObject selectedObject;
        private Scene selectedScene;

        public SceneDebugger() : base(400, null, KeyCode.S, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            ActiveTab = AddTab("Scenes", RenderTabScenes);
            AddTab("Hierarchy", RenderTabHierarchy);
            AddTab("GameObject", RenderTabGameObject);
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

            skin.SetCustomStyle("breadcrumb", skin.label, s =>
            {
                s.fontSize = 20;
                s.fontStyle = FontStyle.Bold;
            });

            skin.SetCustomStyle("breadcrumbNav", skin.box, s =>
            {
                s.stretchWidth = false;
                s.fixedWidth = 100;
            });
        }

        private void RenderTabGameObject()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                if (selectedObject)
                {
                    using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(gameObjectScrollPos))
                    {
                        gameObjectScrollPos = scroll.scrollPosition;

                        GUILayout.Label($"GameObject: {selectedObject.name}", "header");

                        // Add transform interface.
                        using (new GUILayout.VerticalScope("Box"))
                        {
                            GUILayout.Label("Transform");
                            using (new GUILayout.HorizontalScope())
                            {
                                // TODO: Create a "save" button to save changes instead of realtime editing.
                                Vector3 pos = selectedObject.transform.position;
                                float.TryParse(GUILayout.TextField(pos.x.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.x);
                                float.TryParse(GUILayout.TextField(pos.y.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.y);
                                float.TryParse(GUILayout.TextField(pos.z.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out pos.z);
                                selectedObject.transform.position = pos;
                            }
                        }

                        // Add other component interfaces.
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
                    GUILayout.Label($"No selected GameObject\nClick on an object in '{GetTab("Hierarchy").Get().Name}'", "fillMessage");
                }
            }
        }

        private void RenderTabHierarchy()
        {
            using (new GUILayout.HorizontalScope("Box"))
            {
                StringBuilder breadcrumbBuilder = new StringBuilder();
                if (selectedObject != null)
                {
                    Transform parent = selectedObject.transform;
                    while (parent != null)
                    {
                        breadcrumbBuilder.Insert(0, '/');
                        breadcrumbBuilder.Insert(0, string.IsNullOrEmpty(parent.name) ? "<no-name>" : parent.name);
                        parent = parent.parent;
                    }
                }

                breadcrumbBuilder.Insert(0, "//");
                GUILayout.Label(breadcrumbBuilder.ToString(), "breadcrumb");

                using (new GUILayout.HorizontalScope("breadcrumbNav"))
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
                    GUILayout.Label($"No selected scene\nClick on a Scene in '{GetTab("Hierarchy").Get().Name}'", "fillMessage");
                }
            }
        }

        private void RenderTabScenes()
        {
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
                            ActiveTab = GetTab("Hierarchy").Get();
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
        }
    }
}
