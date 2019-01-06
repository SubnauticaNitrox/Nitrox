using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NitroxModel.Core;
using NitroxClient.GameLogic;

namespace NitroxClient.Debuggers
{
    public class SceneDebugger : BaseDebugger
    {
        public readonly KeyCode RayCastKey = KeyCode.F9;

        private Vector2 gameObjectScrollPos;
        private Vector2 hierarchyScrollPos;
        private Vector2 monoBehaviourScrollPos;
        private GameObject selectedObject;
        private MonoBehaviour selectedMonoBehaviour;
        private Scene selectedScene;
        private List<DebuggerAction> actionList = new List<DebuggerAction>();
        private bool editMode;
        private bool sendToServer;
        private BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

        private bool selectedObjectActiveSelf;
        private Vector3 selectedObjectPos;
        private Quaternion selectedObjectRot;
        private Vector3 selectedObjectScale;

        public SceneDebugger() : base(500, null, KeyCode.S, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            ActiveTab = AddTab("Scenes", RenderTabScenes);
            AddTab("Hierarchy", RenderTabHierarchy);
            AddTab("GameObject", RenderTabGameObject);
            AddTab("MonoBehaviour", RenderTabMonoBehaviour);
        }

        public override void Update()
        {
            if (Input.GetKeyDown(RayCastKey))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, int.MaxValue);
                
                foreach (RaycastHit hit in hits)
                {
                    // Not using the player layer mask as we should be able to hit remote players.  Simply filter local player.
                    if (hit.transform.gameObject.name != "Player")
                    {
                        selectedObject = hit.transform.gameObject;
                        ActiveTab = GetTab("Hierarchy").Get();
                        break;
                    }
                }
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

            skin.SetCustomStyle("options", skin.textField, s =>
            {
                s.fixedWidth = 200;
                s.margin = new RectOffset(8, 8, 4, 4);
            });
            skin.SetCustomStyle("options_label", skin.label, s =>
            {
                s.alignment = TextAnchor.MiddleLeft;
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
                        RenderToggleButtons($"GameObject: {selectedObject.name}");
                        
                        using (new GUILayout.VerticalScope("Box"))
                        {
                            using (new GUILayout.HorizontalScope())
                            {
                                selectedObjectActiveSelf = GUILayout.Toggle(selectedObjectActiveSelf, "Active");
                            }
                            GUILayout.Label("Position");
                            using (new GUILayout.HorizontalScope())
                            {
                                float.TryParse(GUILayout.TextField(selectedObjectPos.x.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectPos.x);
                                float.TryParse(GUILayout.TextField(selectedObjectPos.y.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectPos.y);
                                float.TryParse(GUILayout.TextField(selectedObjectPos.z.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectPos.z);
                            }
                            GUILayout.Label("Rotation");
                            using (new GUILayout.HorizontalScope())
                            {
                                float.TryParse(GUILayout.TextField(selectedObjectRot.x.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectRot.x);
                                float.TryParse(GUILayout.TextField(selectedObjectRot.y.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectRot.y);
                                float.TryParse(GUILayout.TextField(selectedObjectRot.z.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectRot.z);
                                float.TryParse(GUILayout.TextField(selectedObjectRot.w.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectRot.w);
                            }
                            GUILayout.Label("Scale");
                            using (new GUILayout.HorizontalScope())
                            {
                                float.TryParse(GUILayout.TextField(selectedObjectScale.x.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectScale.x);
                                float.TryParse(GUILayout.TextField(selectedObjectScale.y.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectScale.y);
                                float.TryParse(GUILayout.TextField(selectedObjectScale.z.ToString()), NumberStyles.Float, CultureInfo.InvariantCulture, out selectedObjectScale.z);
                            }
                        }

                        using (new GUILayout.VerticalScope("Box"))
                        {
                            foreach (MonoBehaviour behaviour in selectedObject.GetComponents<MonoBehaviour>())
                            {
                                Type script = behaviour.GetType();

                                using (new GUILayout.HorizontalScope("Box"))
                                {
                                    if (GUILayout.Button(script.Name))
                                    {
                                        selectedMonoBehaviour = behaviour;
                                        ActiveTab = GetTab("MonoBehaviour").Get();
                                    }
                                }
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

        private void RenderTabMonoBehaviour()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                if (selectedMonoBehaviour)
                {
                    using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(monoBehaviourScrollPos))
                    {
                        monoBehaviourScrollPos = scroll.scrollPosition;
                        RenderToggleButtons($"MonoBehaviour: {selectedMonoBehaviour.GetType().Name}");

                        using (new GUILayout.VerticalScope("Box"))
                        {
                            DisplayAllPublicValues(selectedMonoBehaviour);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"No selected MonoBehaviour\nClick on an object in '{GetTab("MonoBehaviour").Get().Name}'", "fillMessage");
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
                        selectedObjectActiveSelf = selectedObject.activeSelf;
                        selectedObjectPos = selectedObject.transform.position;
                        selectedObjectRot = selectedObject.transform.rotation;
                        selectedObjectScale = selectedObject.transform.localScale;
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
                                selectedObjectActiveSelf = selectedObject.activeSelf;
                                selectedObjectPos = selectedObject.transform.position;
                                selectedObjectRot = selectedObject.transform.rotation;
                                selectedObjectScale = selectedObject.transform.localScale;
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

        private void DisplayAllPublicValues(MonoBehaviour mono)
        {
            FieldInfo[] fields = mono.GetType().GetFields(flags);
            foreach (FieldInfo field in fields)
            {
                using (new GUILayout.HorizontalScope("box"))
                {
                    string[] FieldTypeNames = field.FieldType.ToString().Split('.');
                    GUILayout.Label("[" + FieldTypeNames[FieldTypeNames.Length - 1] + "]: " + field.Name, "options_label");
                    if (field.FieldType == typeof(bool))
                    {
                        bool i = bool.Parse(GetValue(field, selectedMonoBehaviour));
                        if (GUILayout.Button(i.ToString()))
                        {
                            RegistrateChanges(field, selectedMonoBehaviour, !i);
                        }

                    }
                    else if (field.FieldType.BaseType == typeof(MonoBehaviour))
                    {
                        if (GUILayout.Button(field.Name))
                        {
                            selectedMonoBehaviour = (MonoBehaviour)field.GetValue(selectedMonoBehaviour);
                        }

                    }
                    else if (field.FieldType == typeof(GameObject))
                    {
                        if (GUILayout.Button(field.Name))
                        {
                            selectedObject = (GameObject)field.GetValue(selectedMonoBehaviour);
                            ActiveTab = GetTab("GameObject").Get();
                        }

                    }
                    else if (field.FieldType == typeof(Text))
                    {
                        if (GUILayout.Button(field.Name))
                        {
                            selectedMonoBehaviour = (Text)field.GetValue(selectedMonoBehaviour);
                        }
                    }
                    else if (field.FieldType == typeof(Texture) || field.FieldType == typeof(RawImage) || field.FieldType == typeof(Image))
                    {
                        Texture img;
                        if (field.FieldType == typeof(RawImage))
                        {
                            img = ((RawImage)field.GetValue(selectedMonoBehaviour)).mainTexture;
                        }
                        else if (field.FieldType == typeof(Image))
                        {
                            img = ((Image)field.GetValue(selectedMonoBehaviour)).mainTexture;
                        }
                        else
                        {
                            img = (Texture)field.GetValue(selectedMonoBehaviour);
                        }
                        GUIStyle style = new GUIStyle("box");
                        style.fixedHeight = 250 * (img.width / img.height);
                        style.fixedWidth = 250;

                        GUILayout.Box(img, style);
                    }
                    else
                    {
                        try
                        {
                            //Check if convert work to prevent two TextFields
                            Convert.ChangeType(field.GetValue(selectedMonoBehaviour).ToString(), field.FieldType);
                            RegistrateChanges(field, selectedMonoBehaviour, Convert.ChangeType(GUILayout.TextField(GetValue(field, selectedMonoBehaviour), "options"), field.FieldType));
                        }
                        catch
                        {
                            GUILayout.TextField("Not implemented yet", "options");
                        }
                    }
                }
            }
        }

        private void RenderToggleButtons(string label)
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, "header");
                editMode = GUILayout.Toggle(editMode, "EditMode");
                sendToServer = Multiplayer.Main != null && GUILayout.Toggle(sendToServer, "Send to server");
                if (GUILayout.Button("Save"))
                {
                    SaveChanges();
                }
            }
        }

        private void RegistrateChanges(FieldInfo field, Component component, object value)
        {
            if (editMode)
            {
                if (DebuggerAction.GetActionFromField(actionList, field) == null)
                {
                    DebuggerAction action = new DebuggerAction(component, field, component, value);
                    actionList.Add(action);
                }
                else
                {
                    DebuggerAction.GetActionFromField(actionList, field).Value = value;
                }
            }
        }

        private string GetValue(FieldInfo field, object instance)
        {
            if (editMode)
            {
                foreach (DebuggerAction item in actionList)
                {
                    if (item.Field == field)
                    {
                        return item.Value.ToString();
                    }
                }
            }
            return field.GetValue(instance).ToString();
        }

        private void SaveChanges()
        {
            selectedObject.SetActive(selectedObjectActiveSelf);
            selectedObject.transform.position = selectedObjectPos;
            selectedObject.transform.rotation = selectedObjectRot;
            selectedObject.transform.localScale = selectedObjectScale;
            if (sendToServer)
            {
                DebuggerAction.SendValueChangeToServer(selectedObject.transform, "enabled", selectedObjectActiveSelf);
                DebuggerAction.SendValueChangeToServer(selectedObject.transform, "position", selectedObjectPos);
                DebuggerAction.SendValueChangeToServer(selectedObject.transform, "rotation", selectedObjectRot);
                DebuggerAction.SendValueChangeToServer(selectedObject.transform, "scale", selectedObjectScale);
            }

            foreach (DebuggerAction action in actionList)
            {
                action.SaveFieldValue();
                if (sendToServer)
                {
                    action.SendValueChangeToServer();
                }
            }
            actionList.Clear();
        }
    }

    public class DebuggerAction
    {
        public Component Component;
        public FieldInfo Field;
        public object Obj;
        public object Value;

        public DebuggerAction(Component component, FieldInfo field, object obj, object value)
        {
            Validate.NotNull(component);
            Validate.NotNull(field);
            Validate.NotNull(obj);

            Component = component;
            Field = field;
            Obj = obj;
            Value = value;
        }


        public void SaveFieldValue()
        {
            Field.SetValue(Obj, Value);
        }

        public void SendValueChangeToServer()
        {
            if (Multiplayer.Main != null)
            {
                string guid = GetGameObjectPath(Component.gameObject);
                int objectNumber = Component.gameObject.transform.GetSiblingIndex();
                int componentNumber = GetComponentChildNumber(Component);
                NitroxServiceLocator.LocateService<Debugger>().SceneDebuggerChange(guid, objectNumber, componentNumber, Field.Name, Value);
            }
        }

        public static void SendValueChangeToServer(Component component, string fieldName, object value)
        {
            if (Multiplayer.Main != null)
            {
                Validate.NotNull(component);
                Validate.NotNull(fieldName);

                string path = GetGameObjectPath(component.gameObject);
                int objectNumber = component.gameObject.transform.GetSiblingIndex();
                int componentNumber = GetComponentChildNumber(component);
                NitroxServiceLocator.LocateService<Debugger>().SceneDebuggerChange(path, objectNumber, componentNumber, fieldName, value);
            }
        }

        public static DebuggerAction GetActionFromField(List<DebuggerAction> list, FieldInfo field)
        {
            foreach (DebuggerAction item in list)
            {
                if (item.Field == field)
                {
                    return item;
                }
            }
            return null;
            //return list.First(item => item.Field == field);
        }

        private static string GetGameObjectPath(GameObject gameObject)
        {
            StringBuilder path = new StringBuilder("/" + gameObject.name);
            Transform parent = gameObject.transform;
            while (parent.parent != null)
            {
                parent = parent.parent;
                path.Insert(1, parent.name + "/");
            }
            return path.ToString();
        }

        private static int GetComponentChildNumber(Component component)
        {
            int childNumber = -1;
            Component[] components = component.gameObject.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == component)
                {
                    childNumber = i;
                }
            }
            return childNumber;
        }        

    }
}
