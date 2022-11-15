using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using NitroxClient.Debuggers.Drawer;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public class SceneDebugger : BaseDebugger
{
    public GameObject SelectedObject { get; private set; }
    private int selectedComponentID;
    private Scene selectedScene;

    private bool showUnityMethods;
    private bool showSystemMethods;

    private Vector2 gameObjectScrollPos;
    private Vector2 hierarchyScrollPos;

    private readonly Dictionary<Type, IDrawer> debuggerDrawers = new();
    private readonly Dictionary<Type, IStructDrawer> structDebuggerDrawers = new();

    private readonly Dictionary<int, bool> componentsVisibilityByID = new();
    private readonly Dictionary<int, FieldInfo[]> cachedFieldsByComponentID = new();
    private readonly Dictionary<int, MethodInfo[]> cachedMethodsByComponentID = new();

    public SceneDebugger(IEnumerable<IDrawer> drawers, IEnumerable<IStructDrawer> structDrawers) : base(650, null, KeyCode.S, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
    {
        ActiveTab = AddTab("Scenes", RenderTabScenes);
        AddTab("Hierarchy", RenderTabHierarchy);
        AddTab("GameObject", RenderTabGameObject);

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

    protected override void OnSetSkin(GUISkin skin)
    {
        base.OnSetSkin(skin);

        skin.SetCustomStyle("sceneLoaded", skin.label, s =>
        {
            s.normal = new GUIStyleState { textColor = Color.green };
            s.fontStyle = FontStyle.Bold;
        });

        skin.SetCustomStyle("loadScene", skin.button, s => { s.fixedWidth = 60; });

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
        using (new GUILayout.VerticalScope("box"))
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

                using (new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button($"{(isSelected ? ">> " : "")}{i}: {(isDDOLScene ? currentScene.name : path.TruncateLeft(35))}", isLoaded ? "sceneLoaded" : "label"))
                    {
                        selectedScene = currentScene;
                        ActiveTab = GetTab("Hierarchy").Value;
                    }

                    if (isLoaded)
                    {
                        if (!isDDOLScene && GUILayout.Button("Unload", "loadScene"))
                        {
                            SceneManager.UnloadSceneAsync(i);
                        }
                    }
                    else
                    {
                        if (!isDDOLScene && GUILayout.Button("Load", "loadScene"))
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
        using (new GUILayout.HorizontalScope("box"))
        {
            StringBuilder breadcrumbBuilder = new();
            if (SelectedObject)
            {
                Transform parent = SelectedObject.transform;
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
                    UpdateSelectedObject(null);
                }

                if (GUILayout.Button("<") && SelectedObject && SelectedObject.transform.parent)
                {
                    UpdateSelectedObject(SelectedObject.transform.parent.gameObject);
                }
            }
        }

        using (new GUILayout.VerticalScope("box"))
        {
            if (selectedScene.IsValid())
            {
                using GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos);
                hierarchyScrollPos = scroll.scrollPosition;
                List<GameObject> showObjects = new();
                if (!SelectedObject)
                {
                    showObjects = selectedScene.GetRootGameObjects().ToList();
                }
                else
                {
                    foreach (Transform t in SelectedObject.transform)
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

    private void RenderTabGameObject()
    {
        using (new GUILayout.VerticalScope("box"))
        {
            if (!SelectedObject)
            {
                GUILayout.Label($"No selected GameObject\nClick on an object in '{GetTab("Hierarchy").Value.Name}'", "fillMessage");
                return;
            }

            using GUILayout.ScrollViewScope scroll = new(gameObjectScrollPos);
            gameObjectScrollPos = scroll.scrollPosition;

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"GameObject: {SelectedObject.name}", "bold", GUILayout.Height(25));
                GUILayout.Space(5);
            }

            foreach (Component component in SelectedObject.GetComponents<Component>())
            {
                int componentId = component.GetInstanceID();

                using (new GUILayout.VerticalScope(selectedComponentID == componentId ? "boxHighlighted" : "box"))
                {
                    if (!componentsVisibilityByID.TryGetValue(componentId, out bool visible))
                    {
                        visible = componentsVisibilityByID[componentId] = component is Transform or RectTransform; // Transform should be visible by default
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

    private void DrawFields(UnityEngine.Object target)
    {
        if (!cachedFieldsByComponentID.TryGetValue(target.GetInstanceID(), out FieldInfo[] fields))
        {
            fields = cachedFieldsByComponentID[target.GetInstanceID()] = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
        }

        foreach (FieldInfo field in fields)
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
                        case ScriptableObject scriptableObject:
                            if (GUILayout.Button(field.Name))
                            {
                                DrawFields(scriptableObject);
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
        using (new GUILayout.HorizontalScope("box"))
        {
            showSystemMethods = GUILayout.Toggle(showSystemMethods, "Show System inherit methods", GUILayout.Height(25));
            showUnityMethods = GUILayout.Toggle(showUnityMethods, "Show Unity inherit methods", GUILayout.Height(25));
        }

        if (!cachedMethodsByComponentID.TryGetValue(monoBehaviour.GetInstanceID(), out MethodInfo[] methods))
        {
            methods = cachedMethodsByComponentID[monoBehaviour.GetInstanceID()] = monoBehaviour.GetType().GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                                                                                               .OrderBy(m => m.Name).ToArray();
        }

        foreach (MethodInfo method in methods)
        {
            string methodAssemblyName = method.DeclaringType != null ? method.DeclaringType.Assembly.GetName().Name : string.Empty;
            if (!(!showSystemMethods && (methodAssemblyName.Contains("System") || methodAssemblyName.Contains("mscorlib"))) &&
                !(!showUnityMethods && methodAssemblyName.Contains("UnityEngine")))
            {
                using (new GUILayout.VerticalScope("box"))
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
        if (SelectedObject == item)
        {
            return;
        }

        SelectedObject = item;
        selectedComponentID = default;
    }

    public void JumpToComponent(Component item)
    {
        UpdateSelectedObject(item.gameObject);
        RenderTabGameObject();
        selectedComponentID = item.GetInstanceID();
    }
}
