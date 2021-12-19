using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NitroxClient.Debuggers.Drawer;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mathf = UnityEngine.Mathf;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public class SceneDebugger : BaseDebugger
{
    private const KeyCode RAY_CAST_KEY = KeyCode.F9;
    private bool hitMode;

    private bool showUnityMethods;
    private bool showSystemMethods;

    private Vector2 gameObjectScrollPos;
    private Vector2 hierarchyScrollPos;

    private string gameObjectSearch = "";
    private string gameObjectSearchCache = "";
    private bool gameObjectSearchIsSearching;
    private string gameObjectSearchPatternInvalidMessage = "";
    private List<GameObject> gameObjectSearchResult = new();

    private Scene selectedScene;
    private GameObject selectedObject;
    private int selectedComponentID;

    private readonly Dictionary<Type, IDrawer> debuggerDrawers = new();
    private readonly Dictionary<Type, IStructDrawer> structDebuggerDrawers = new();

    private readonly Dictionary<int, bool> componentsVisibilityByID = new();

    private readonly Texture arrowTexture;
    private readonly Texture circleTexture;

    public SceneDebugger(IEnumerable<IDrawer> drawers, IEnumerable<IStructDrawer> structDrawers) : base(700, null, KeyCode.S, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
    {
        ActiveTab = AddTab("Scenes", RenderTabScenes);
        AddTab("Hierarchy", RenderTabHierarchy);
        AddTab("GameObject", RenderTabGameObject);

        if (Multiplayer.Active) // circleTexture wont load in main menu
        {
            arrowTexture = Resources.Load<Texture2D>("Sprites/Arrow");
            circleTexture = Resources.Load<Material>("Materials/WorldCursor").GetTexture("_MainTex");
        }

        foreach (IDrawer drawer in drawers)
        {
            foreach (Type type in drawer.ApplicableTypes)
            {
                debuggerDrawers.Add(type, drawer);
            }
        }

        foreach (IStructDrawer structDrawer in structDrawers)
        {
            foreach (Type type in structDrawer.ApplicableTypes)
            {
                structDebuggerDrawers.Add(type, structDrawer);
            }
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (!selectedObject)
        {
            return;
        }

        UpdateSelectedObjectMarker();
    }

    private void UpdateSelectedObjectMarker()
    {
        Texture currentTexture;
        float markerX, markerY, markerRot;

        Vector3 screenPos = Player.main.viewModelCamera.WorldToScreenPoint(selectedObject.transform.position);
        //if object is on screen
        if (screenPos.z > 0 &&
            screenPos.x >= 0 && screenPos.x < Screen.width &&
            screenPos.y >= 0 && screenPos.y < Screen.height)
        {
            currentTexture = circleTexture;
            markerX = screenPos.x;
            //subtract from height to go from bottom up to top down
            markerY = Screen.height - screenPos.y;
            markerRot = 0;
        }
        //if object is not on screen
        else
        {
            currentTexture = arrowTexture;
            //if the object is behind us, flip across the center
            if (screenPos.z < 0)
            {
                screenPos.x = Screen.width - screenPos.x;
                screenPos.y = Screen.height - screenPos.y;
            }

            //calculate new position of arrow (somewhere on the edge)
            Vector3 screenCenter = new Vector3(Screen.width, Screen.height, 0) / 2f;
            Vector3 originPos = screenPos - screenCenter;

            float angle = Mathf.Atan2(originPos.y, originPos.x) - (90 * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float m = cos / -sin;

            Vector3 screenBounds = screenCenter * 0.9f;

            screenPos = cos > 0 ? new Vector3(screenBounds.y / m, screenBounds.y, 0) : new Vector3(-screenBounds.y / m, -screenBounds.y, 0);

            if (screenPos.x > screenBounds.x)
            {
                screenPos = new Vector3(screenBounds.x, screenBounds.x * m, 0);
            }
            else if (screenPos.x < -screenBounds.x)
            {
                screenPos = new Vector3(-screenBounds.x, -screenBounds.x * m, 0);
            }

            screenPos += screenCenter;

            markerX = screenPos.x;
            markerY = Screen.height - screenPos.y;
            markerRot = -angle * Mathf.Rad2Deg;
        }

        float markerSizeX = currentTexture.width;
        float markerSizeY = currentTexture.height;
        GUI.matrix = Matrix4x4.Translate(new Vector3(markerX, markerY, 0)) *
                     Matrix4x4.Rotate(Quaternion.Euler(0, 0, markerRot)) *
                     Matrix4x4.Scale(new Vector3(0.5f, 0.5f, 0.5f)) *
                     Matrix4x4.Translate(new Vector3(-markerSizeX / 2, -markerSizeY / 2, 0));

        GUI.DrawTexture(new Rect(0, 0, markerSizeX, markerSizeY), currentTexture);
        GUI.matrix = Matrix4x4.identity;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(RAY_CAST_KEY))
        {
            //second press disables hitMode and goes back to default scene object list in hierarchy
            if (hitMode)
            {
                gameObjectSearchResult.Clear();
                hitMode = false;
            }
            else
            {
                gameObjectSearchResult.Clear();
                hitMode = true;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, int.MaxValue);

                foreach (RaycastHit hit in hits)
                {
                    gameObjectSearchResult.Add(hit.transform.gameObject);
                }

                ActiveTab = GetTab("Hierarchy").Value;
            }
        }
    }

    protected override void OnSetSkin(GUISkin skin)
    {
        base.OnSetSkin(skin);

        skin.SetCustomStyle("sceneLoaded", skin.label, s =>
        {
            s.normal = new GUIStyleState { textColor = Color.green };
            s.fontStyle = FontStyle.Bold;
        });

        skin.SetCustomStyle("loadScene", skin.button, s => { s.fixedWidth = 60; });

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
        skin.SetCustomStyle("options_label", skin.label, s => { s.alignment = TextAnchor.MiddleLeft; });

        skin.SetCustomStyle("bold", skin.label, s =>
        {
            s.alignment = TextAnchor.LowerLeft;
            s.fontStyle = FontStyle.Bold;
        });
        skin.SetCustomStyle("error", skin.label, s =>
        {
            s.fontStyle = FontStyle.Bold;
            s.normal.textColor = Color.red;
        });
        skin.SetCustomStyle("boxHighlighted", skin.box, s =>
        {
            Texture2D result = new(1, 1);
            result.SetPixels(new[] { new Color(1f, 0.9f, 0f, 0.25f) });
            result.Apply();
            s.normal.background = result;
        });
    }

    private void RenderTabScenes()
    {
        using (new GUILayout.VerticalScope("Box"))
        {
            GUILayout.Label("All scenes", "header");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings + 1; i++)
            {
                Scene currentScene;
                string path = "";
                if (i == SceneManager.sceneCountInBuildSettings)
                {
                    currentScene = NitroxBootstrapper.Instance.gameObject.scene; // bit of a hack
                }
                else
                {
                    currentScene = SceneManager.GetSceneByBuildIndex(i);
                    path = SceneUtility.GetScenePathByBuildIndex(i);
                }

                bool isSelected = selectedScene.IsValid() && currentScene == selectedScene;
                bool isLoaded = currentScene.isLoaded;
                bool isDDOLScene = currentScene.name == "DontDestroyOnLoad";

                using (new GUILayout.HorizontalScope("Box"))
                {
                    if (GUILayout.Button($"{(isSelected ? ">> " : "")}{i}: {(isDDOLScene ? currentScene.name : path.TruncateLeft(35))}", isLoaded ? "sceneLoaded" : "label"))
                    {
                        selectedScene = currentScene;
                        ActiveTab = GetTab("Hierarchy").Value;
                    }

                    if (isLoaded)
                    {
                        if (!isDDOLScene && GUILayout.Button("Load", "loadScene"))
                        {
                            SceneManager.UnloadSceneAsync(i);
                        }
                    }
                    else
                    {
                        if (!isDDOLScene && GUILayout.Button("Unload", "loadScene"))
                        {
                            SceneManager.LoadSceneAsync(i);
                        }
                    }
                }
            }
        }
    }

    private void RenderTabHierarchy()
    {
        using (new GUILayout.HorizontalScope("Box"))
        {
            StringBuilder breadcrumbBuilder = new();
            if (selectedObject)
            {
                Transform parent = selectedObject.transform;
                while (parent)
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

                if (GUILayout.Button("<") && selectedObject && selectedObject.transform.parent)
                {
                    UpdateSelectedObject(selectedObject.transform.parent.gameObject);
                }
            }
        }

        // GameObject search textbox.
        using (new GUILayout.HorizontalScope("Box"))
        {
            gameObjectSearch = GUILayout.TextField(gameObjectSearch);

            // Disable searching if text is cleared after a search has happened.
            if (gameObjectSearchIsSearching && string.IsNullOrEmpty(gameObjectSearch))
            {
                gameObjectSearchIsSearching = false;
            }

            if (gameObjectSearch.Length > 0)
            {
                if (GUILayout.Button("Search", "button", GUILayout.Width(80)))
                {
                    gameObjectSearchIsSearching = true;
                }
                else if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    gameObjectSearchIsSearching = true;
                }
            }
        }

        if (hitMode)
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                if (gameObjectSearchResult.Count > 0)
                {
                    using GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos);
                    hierarchyScrollPos = scroll.scrollPosition;
                    foreach (GameObject child in gameObjectSearchResult)
                    {
                        string guiStyle = child.transform.childCount > 0 ? "bold" : "label";
                        if (GUILayout.Button($"{child.name}", guiStyle))
                        {
                            UpdateSelectedObject(child);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"No selected scene\nClick on a Scene in '{GetTab("Hierarchy").Value.Name}'", "fillMessage");
                }
            }
        }
        else if (!gameObjectSearchIsSearching)
        {
            // Not searching, just select game objects from selected scene (if any).
            using (new GUILayout.VerticalScope("Box"))
            {
                if (selectedScene.IsValid())
                {
                    using GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos);
                    hierarchyScrollPos = scroll.scrollPosition;
                    List<GameObject> showObjects = new();
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
                        if (GUILayout.Button(child.name, child.transform.childCount > 0 ? "bold" : "label"))
                        {
                            UpdateSelectedObject(child);
                        }
                    }
                }
                else
                {
                    GUILayout.Label($"No selected scene\nClick on a Scene in '{GetTab("Hierarchy").Value.Name}'", "fillMessage");
                }
            }
        }
        else
        {
            // Searching. Return all gameobjects with matching type name.
            if (gameObjectSearch != gameObjectSearchCache && gameObjectSearch.Length > 2)
            {
                try
                {
                    Regex.IsMatch("", gameObjectSearch);
                    gameObjectSearchPatternInvalidMessage = "";
                }
                catch (Exception ex)
                {
                    gameObjectSearchPatternInvalidMessage = ex.Message;
                }

                if (string.IsNullOrEmpty(gameObjectSearchPatternInvalidMessage))
                {
                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                                         .Select(a => a.GetType(gameObjectSearch, false, true))
                                         .FirstOrDefault(t => t != null);
                    if (type != null)
                    {
                        List<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                                                                .Where(g => g.GetComponent(type))
                                                                .ToList();

                        gameObjectSearchResult = gameObjects;
                    }
                    else
                    {
                        gameObjectSearchResult = Resources.FindObjectsOfTypeAll<GameObject>().Where(go => Regex.IsMatch(go.name, gameObjectSearch, RegexOptions.IgnoreCase)).OrderBy(go => go.name).ToList();
                    }

                    gameObjectSearchCache = gameObjectSearch;
                }
                else
                {
                    GUILayout.Label(gameObjectSearchPatternInvalidMessage, "error");
                }
            }

            using GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos);
            hierarchyScrollPos = scroll.scrollPosition;
            foreach (GameObject item in gameObjectSearchResult)
            {
                if (item != null)
                {
                    string guiStyle = item.transform.childCount > 0 ? "bold" : "label";
                    if (GUILayout.Button($"{item.name}", guiStyle))
                    {
                        UpdateSelectedObject(item);
                    }
                }
            }
        }
    }

    private void RenderTabGameObject()
    {
        using (new GUILayout.VerticalScope("Box"))
        {
            if (!selectedObject)
            {
                GUILayout.Label($"No selected GameObject\nClick on an object in '{GetTab("Hierarchy").Value.Name}'", "fillMessage");
                return;
            }

            using GUILayout.ScrollViewScope scroll = new(gameObjectScrollPos);
            gameObjectScrollPos = scroll.scrollPosition;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"GameObject: {selectedObject.name}", "bold", GUILayout.Height(25));
                GUILayout.Space(5);
            }

            foreach (Component component in selectedObject.GetComponents<Component>())
            {
                int componentId = component.GetInstanceID();

                using (new GUILayout.VerticalScope(selectedComponentID == componentId ? "boxHighlighted" : "box"))
                {
                    if (!componentsVisibilityByID.TryGetValue(componentId, out bool visible))
                    {
                        visible = componentsVisibilityByID[componentId] = true;
                    }

                    Type componentType = component.GetType();
                    MonoBehaviour monoBehaviour = component as MonoBehaviour;

                    using (new GUILayout.HorizontalScope(GUILayout.Height(12f)))
                    {
                        if (monoBehaviour)
                        {
                            monoBehaviour.enabled = GUILayout.Toggle(monoBehaviour.enabled, GUIContent.none, GUILayout.Width(15));
                        }

                        GUILayout.Label(componentType.Name, "bold", GUILayout.Height(20));
                        NitroxGUILayout.Separator();

                        if (GUILayout.Button("Show / Hide", GUILayout.Width(100)))
                        {
                            componentsVisibilityByID[component.GetInstanceID()] = visible = !visible;
                        }
                    }

                    GUILayout.Space(5);

                    if (visible)
                    {
                        GUILayout.Space(10);
                        if (debuggerDrawers.TryGetValue(componentType, out IDrawer drawer))
                        {
                            drawer.Draw(component);
                        }
                        else if (monoBehaviour)
                        {
                            DrawFields(monoBehaviour);
                            GUILayout.Space(20);
                            DrawMonoBehaviourMethods(monoBehaviour);
                        }
                        else
                        {
                            NitroxGUILayout.Separator();
                            GUILayout.Label("This component is not yet supported");
                            GUILayout.Space(10);
                        }
                    }

                    GUILayout.Space(3);
                }
            }
        }
    }

    private void DrawFields(object target)
    {
        foreach (FieldInfo field in target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
        {
            object fieldValue = field.GetValue(target);

            using (new GUILayout.HorizontalScope("box", GUILayout.MinHeight(35)))
            {
                GUILayout.Label($"[{field.FieldType.ToString().Split('.').Last()}]: {field.Name}", "options_label");
                NitroxGUILayout.Separator();

                if (fieldValue is Component component)
                {
                    if (GUILayout.Button("Goto"))
                    {
                        JumpToComponent(component);
                    }
                }
                else if (debuggerDrawers.TryGetValue(field.FieldType, out IDrawer drawer))
                {
                    drawer.Draw(fieldValue);
                }
                else if (structDebuggerDrawers.TryGetValue(field.FieldType, out IStructDrawer structDrawer))
                {
                    field.SetValue(target, structDrawer.Draw(fieldValue));
                }
                else
                {
                    switch (fieldValue)
                    {
                        case null:
                            GUILayout.Box("Field is null");
                            break;
                        case ScriptableObject:
                            if (GUILayout.Button(field.Name))
                            {
                                DrawFields(fieldValue);
                            }

                            break;
                        case GameObject gameObject:
                            if (GUILayout.Button(field.Name))
                            {
                                UpdateSelectedObject(gameObject);
                            }

                            break;
                        case bool boolValue:
                            if (GUILayout.Button(boolValue.ToString()))
                            {
                                field.SetValue(target, !boolValue);
                            }

                            break;
                        case short:
                        case ushort:
                        case int:
                        case uint:
                        case long:
                        case ulong:
                        case float:
                        case double:
                            field.SetValue(target, NitroxGUILayout.ConvertibleField((IConvertible)fieldValue));
                            break;
                        case Enum enumValue:
                            NitroxGUILayout.EnumPopup(enumValue);
                            break;
                        default:
                            GUILayout.TextArea(fieldValue.ToString(), "options");
                            break;
                    }
                }
            }
        }
    }

    private void DrawMonoBehaviourMethods(MonoBehaviour monoBehaviour)
    {
        using (new GUILayout.HorizontalScope("Box"))
        {
            showSystemMethods = GUILayout.Toggle(showSystemMethods, "Show System inherit methods", GUILayout.Height(25));
            showUnityMethods = GUILayout.Toggle(showUnityMethods, "Show Unity inherit methods", GUILayout.Height(25));
        }

        MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).OrderBy(m => m.Name).ToArray();
        foreach (MethodInfo method in methods)
        {
            string methodAssemblyName = method.DeclaringType.Assembly.GetName().Name;
            if (!(!showSystemMethods && (methodAssemblyName.Contains("System") || methodAssemblyName.Contains("mscorlib"))) &&
                !(!showUnityMethods && methodAssemblyName.Contains("UnityEngine")))
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    GUILayout.Label(method.ToString());

                    if (method.GetParameters().Any()) // TODO: Allow methods with parameters to be called.
                    {
                        continue;
                    }

                    if (GUILayout.Button("Invoke", GUILayout.MaxWidth(150)))
                    {
                        object result = method.Invoke(method.IsStatic ? null : monoBehaviour, Array.Empty<object>());
                        Log.InGame($"Invoked method {method.Name}");

                        if (method.ReturnType != typeof(void))
                        {
                            Log.InGame(result != null ? $"Returned: '{result}'" : "Return value was NULL.");
                        }
                    }
                }
            }
        }
    }

    public void UpdateSelectedObject(GameObject item)
    {
        if (item == selectedObject)
        {
            return;
        }

        selectedObject = item.gameObject;
    }

    public void JumpToComponent(Component item)
    {
        UpdateSelectedObject(item.gameObject);
        RenderTabGameObject();
        selectedComponentID = item.GetInstanceID();
    }
}
