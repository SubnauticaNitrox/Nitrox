using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Nitrox.Model.DataStructures;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UWE;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public sealed class PrefabDebugger : AbstractDebugger
{
    private const int WINDOW_ID = 426;
    private const int PREVIEW_LAYER = 31;
    private const float PREFAB_ROW_HEIGHT = 42f;
    private const float PREFAB_LIST_HEIGHT = 720f;
    private const float PREVIEW_HEIGHT = 540f;
    private const float AXIS_GIZMO_SIZE = 54f;
    private const float MAX_PREVIEW_DISTANCE = 100f;
    private const float MIN_PREVIEW_DISTANCE = 0.05f;
    private const float MIN_PREVIEW_RADIUS = 0.01f;
    private const string UNKNOWN_TECH_TYPE = "Unknown";

    private readonly SceneDebugger sceneDebugger;
    private readonly List<string> prefabClassIds = [];
    private readonly List<string> filteredClassIds = [];
    private readonly List<GameObject> spawnedObjects = [];
    private readonly Dictionary<string, string> prefabNameByClassId = [];
    private readonly Dictionary<string, string> techTypeByClassId = [];
    private Vector2 prefabScrollPosition;
    private string searchText = string.Empty;
    private string selectedClassId;
    private GameObject loadedPrefab;
    private GameObject previewObject;
    private Camera previewCamera;
    private RenderTexture previewTexture;
    private float previewYaw = 25f;
    private float previewPitch = 10f;
    private float previewDistance = 3f;
    private float previewDefaultDistance = 3f;
    private Vector3 previewPosition;
    private int previewRequestVersion;
    private int previewControlId;
    private Vector2 previewLastMousePosition;
    private string statusMessage;

    private bool prefabListInitialized;
    private bool previewNeedsRender;
    private bool previewDragging;
    private bool previewMoving;
    private bool previewMovingDepth;
    private bool previewLightBackground;
    private bool previewHasVisibleRenderer;

    public PrefabDebugger(SceneDebugger sceneDebugger) : base(WINDOW_ID, 900, "Prefabs", KeyCode.P, true, false, false, GUISkinCreationOptions.DERIVEDCOPY, 850)
    {
        this.sceneDebugger = sceneDebugger;
        ActiveTab = AddTab("Prefab explorer", RenderPrefabExplorer);
        AddTab("Instantiated prefabs", RenderInstantiatedPrefabs);
    }

    protected override void OnSetSkin(GUISkin skin)
    {
        base.OnSetSkin(skin);
        skin.SetCustomStyle("preview", skin.box, style =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.stretchWidth = true;
            style.stretchHeight = true;
        });
        skin.SetCustomStyle("selectedPrefab", skin.button, static style =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.cyan;
        });
        skin.SetCustomStyle("prefabHeader", skin.label, static style =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Bold;
        });
        skin.SetCustomStyle("axisLabelX", skin.label, static style =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.red;
        });
        skin.SetCustomStyle("axisLabelY", skin.label, static style =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.green;
        });
        skin.SetCustomStyle("axisLabelZ", skin.label, static style =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.blue;
        });
        skin.SetCustomStyle("prefabRow", skin.box, static style =>
        {
            style.margin = new RectOffset(2, 2, 2, 2);
            style.padding = new RectOffset(4, 4, 2, 2);
        });
        skin.SetCustomStyle("prefabRowSelected", skin.box, static style =>
        {
            style.margin = new RectOffset(2, 2, 2, 2);
            style.padding = new RectOffset(4, 4, 2, 2);
            Texture2D background = new(1, 1);
            background.SetPixel(0, 0, new Color(0.1f, 0.5f, 0.7f, 0.45f));
            background.Apply();
            style.normal.background = background;
        });
    }

    private void RenderPrefabExplorer()
    {
        // PrefabDatabase can still be empty while the debugger is constructed.
        // Load it on the first GUI pass instead of doing lifecycle-sensitive work in the constructor.
        if (!prefabListInitialized)
        {
            RefreshPrefabList();
            prefabListInitialized = true;
        }

        using (new GUILayout.HorizontalScope())
        {
            RenderPrefabList();
            RenderPrefabPreview();
        }
    }

    private void RenderInstantiatedPrefabs()
    {
        // Unity-destroyed objects remain in the list as fake-null references until removed.
        spawnedObjects.RemoveAll(gameObject => !gameObject);

        using (new GUILayout.VerticalScope("Box"))
        {
            GUILayout.Label($"Instantiated prefabs ({spawnedObjects.Count})", "prefabHeader");
            if (spawnedObjects.Count == 0)
            {
                GUILayout.Label("No prefabs have been instantiated.");
                return;
            }

            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                GameObject spawnedObject = spawnedObjects[i];
                using (new GUILayout.HorizontalScope("prefabRow"))
                {
                    GUILayout.Label($"#{i + 1} {GetPrefabName(spawnedObject.name)}\n{spawnedObject.name}", GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Inspect", GUILayout.Width(70f)))
                    {
                        sceneDebugger.InspectGameObject(spawnedObject);
                    }
                    if (GUILayout.Button("Destroy", GUILayout.Width(70f)))
                    {
                        string spawnedObjectName = spawnedObject.name;
                        UnityEngine.Object.Destroy(spawnedObject);
                        spawnedObjects.RemoveAt(i);
                        statusMessage = $"Destroyed {spawnedObjectName}.";
                        break;
                    }
                }
            }
        }
    }

    private void RenderPrefabList()
    {
        using (new GUILayout.VerticalScope("Box", GUILayout.Width(360f)))
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label($"Available prefabs ({prefabClassIds.Count})", "prefabHeader", GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Reload", GUILayout.Width(70f)))
                {
                    RefreshPrefabList();
                }
            }
            GUILayout.Space(8f);
            using (new GUILayout.HorizontalScope())
            {
                searchText = GUILayout.TextField(searchText, GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Clear", GUILayout.Width(60f)))
                {
                    searchText = string.Empty;
                    FilterPrefabList();
                }
                if (GUILayout.Button("Search", GUILayout.Width(70f)))
                {
                    FilterPrefabList();
                }
            }

            // Only create controls for visible rows. This prevents a large prefab database
            // from rebuilding hundreds of IMGUI controls every frame.
            using GUILayout.ScrollViewScope scroll = new(prefabScrollPosition, GUILayout.Height(PREFAB_LIST_HEIGHT));
            prefabScrollPosition = scroll.scrollPosition;
            int firstVisibleIndex = Mathf.Max(0, Mathf.FloorToInt(prefabScrollPosition.y / PREFAB_ROW_HEIGHT) - 1);
            int visibleRowCount = Mathf.CeilToInt(PREFAB_LIST_HEIGHT / PREFAB_ROW_HEIGHT) + 2;
            int lastVisibleIndex = Mathf.Min(filteredClassIds.Count, firstVisibleIndex + visibleRowCount);

            GUILayout.Space(firstVisibleIndex * PREFAB_ROW_HEIGHT);
            for (int i = firstVisibleIndex; i < lastVisibleIndex; i++)
            {
                string classId = filteredClassIds[i];
                string displayName = GetPrefabName(classId);
                using (new GUILayout.VerticalScope(classId == selectedClassId ? "prefabRowSelected" : "prefabRow", GUILayout.Height(PREFAB_ROW_HEIGHT)))
                {
                    if (GUILayout.Button($"{displayName}\n{classId}", classId == selectedClassId ? "selectedPrefab" : "label", GUILayout.ExpandHeight(true)))
                    {
                        SelectPrefab(classId);
                    }
                }
            }
            GUILayout.Space(Mathf.Max(0, filteredClassIds.Count - lastVisibleIndex) * PREFAB_ROW_HEIGHT);

            if (filteredClassIds.Count == 0)
            {
                GUILayout.Label("No prefabs found. Try Reload after entering the game.");
            }
        }
    }

    private void RenderPrefabPreview()
    {
        using (new GUILayout.VerticalScope("Box", GUILayout.ExpandHeight(true)))
        {
            RenderReadOnlyField("Name", selectedClassId == null ? string.Empty : GetPrefabName(selectedClassId));
            RenderReadOnlyField("Class ID", selectedClassId ?? string.Empty);
            RenderReadOnlyField("Tech Type", selectedClassId == null ? string.Empty : GetPrefabTechType(selectedClassId));

            if (previewTexture && previewCamera && previewHasVisibleRenderer)
            {
                if (previewNeedsRender)
                {
                    RenderPreview();
                }
                GUI.SetNextControlName("PrefabPreview");
                // Keep the preview height fixed so metadata changes do not shift the render area.
                GUILayout.Box(GUIContent.none, "preview", GUILayout.ExpandWidth(true), GUILayout.Height(PREVIEW_HEIGHT));
                Rect previewRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.StretchToFill, false);
                DrawAxisGizmo(previewRect);
                HandlePreviewMouseInput(previewRect);
            }
            else
            {
                GUILayout.Label(statusMessage ?? "No preview loaded.", "preview", GUILayout.ExpandWidth(true), GUILayout.Height(PREVIEW_HEIGHT));
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                bool guiEnabled = GUI.enabled;
                GUI.enabled = guiEnabled && previewHasVisibleRenderer;
                if (GUILayout.Button("Spawn in front of player", GUILayout.Width(240f)))
                {
                    InstantiateSelectedPrefab();
                    ActiveTab = GetTab("Instantiated prefabs").Value;
                }
                GUI.enabled = guiEnabled;
                GUILayout.FlexibleSpace();
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                GUI.enabled = previewObject;
                if (GUILayout.Button("Inspect in SceneDebugger", GUILayout.Width(240f)))
                {
                    sceneDebugger.InspectGameObject(previewObject);
                }
                GUI.enabled = true;
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(8f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Yaw", GUILayout.Width(35f));
                float yaw = GUILayout.HorizontalSlider(previewYaw, -180f, 180f);
                GUILayout.Label("Pitch", GUILayout.Width(45f));
                float pitch = GUILayout.HorizontalSlider(previewPitch, -180f, 180f);
                previewNeedsRender |= !Mathf.Approximately(yaw, previewYaw) || !Mathf.Approximately(pitch, previewPitch);
                previewYaw = yaw;
                previewPitch = pitch;
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("X", GUILayout.Width(15f));
                float x = GUILayout.HorizontalSlider(previewPosition.x, -5f, 5f);
                GUILayout.Label("Y", GUILayout.Width(15f));
                float y = GUILayout.HorizontalSlider(previewPosition.y, -5f, 5f);
                GUILayout.Label("Z", GUILayout.Width(15f));
                float z = GUILayout.HorizontalSlider(previewPosition.z, -5f, 5f);
                previewNeedsRender |= !Mathf.Approximately(x, previewPosition.x) ||
                                      !Mathf.Approximately(y, previewPosition.y) ||
                                      !Mathf.Approximately(z, previewPosition.z);
                previewPosition = new Vector3(x, y, z);
            }

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Distance", GUILayout.Width(60f));
                float distance = GUILayout.HorizontalSlider(previewDistance, MIN_PREVIEW_DISTANCE, MAX_PREVIEW_DISTANCE);
                previewNeedsRender |= !Mathf.Approximately(distance, previewDistance);
                previewDistance = distance;
            }

            GUILayout.Space(8f);

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset view", GUILayout.Width(90f)))
                {
                    previewYaw = 25f;
                    previewPitch = 10f;
                    previewDistance = previewDefaultDistance;
                    previewPosition = Vector3.zero;
                    previewNeedsRender = true;
                }
                bool guiEnabled = GUI.enabled;
                GUI.enabled = guiEnabled && previewCamera;
                if (GUILayout.Button(previewLightBackground ? "Dark background" : "Light background", GUILayout.Width(125f)))
                {
                    previewLightBackground = !previewLightBackground;
                    ApplyPreviewBackground();
                    previewNeedsRender = true;
                    GUI.changed = true;
                }
                GUI.enabled = guiEnabled;
                GUILayout.FlexibleSpace();
            }

            if (previewHasVisibleRenderer && !string.IsNullOrEmpty(statusMessage))
            {
                GUILayout.Label(statusMessage);
            }
        }
    }

    private void SelectPrefab(string classId)
    {
        if (selectedClassId == classId)
        {
            return;
        }

        selectedClassId = classId;
        DestroyPreview();
        statusMessage = "Loading prefab...";
        int requestVersion = ++previewRequestVersion;
        NitroxBootstrapper.Instance.StartCoroutine(LoadPreview(classId, requestVersion));
    }

    private static void RenderReadOnlyField(string label, string value)
    {
        using (new GUILayout.HorizontalScope())
        {
            GUILayout.Label(label, GUILayout.Width(60f));
            GUILayout.Label(value, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Copy", GUILayout.Width(55f)))
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }
    }

    private void RefreshPrefabList()
    {
        prefabClassIds.Clear();
        prefabNameByClassId.Clear();
        foreach (string classId in GetPrefabClassIds())
        {
            prefabClassIds.Add(classId);
            prefabNameByClassId[classId] = ResolvePrefabName(classId);
        }
        FilterPrefabList();
        statusMessage = prefabClassIds.Count == 0 ? "PrefabDatabase is not populated yet." : null;
    }

    private void FilterPrefabList()
    {
        filteredClassIds.Clear();
        foreach (string classId in prefabClassIds)
        {
            string prefabName = prefabNameByClassId[classId];
            // Search using classID or prefab name
            if (classId.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 || prefabName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                filteredClassIds.Add(classId);
            }
        }
        prefabScrollPosition = Vector2.zero;
    }

    private IEnumerator LoadPreview(string classId, int requestVersion)
    {
        IPrefabRequest request = PrefabDatabase.GetPrefabAsync(classId);
        yield return request;

        // A previous request may finish after the user selected another row.
        // Ignore stale results so an old prefab cannot replace the current preview.
        if (requestVersion != previewRequestVersion || classId != selectedClassId)
        {
            yield break;
        }

        if (!request.TryGetPrefab(out GameObject? prefab) || !prefab)
        {
            statusMessage = "Prefab could not be loaded.";
            yield break;
        }

        bool hasRenderer = CreatePreview(prefab);
        loadedPrefab = prefab;
        techTypeByClassId[classId] = ResolveTechType(prefab);
        statusMessage = hasRenderer ? null : "Prefab has no visible renderer.";
    }

    private bool CreatePreview(GameObject prefab)
    {
        EnsurePreviewCamera();
        // The clone is never part of the game scene: it is isolated on PREVIEW_LAYER
        // and disabled except for the single camera render below.
        previewObject = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, false);
        previewObject.name = $"[Prefab preview] {prefab.name}";
        SetLayerRecursively(previewObject.transform, PREVIEW_LAYER);
        DisablePreviewBehaviours(previewObject);
        previewYaw = 25f;
        previewPitch = 10f;
        previewObject.transform.position = Vector3.zero;
        previewObject.transform.rotation = Quaternion.Euler(previewPitch, previewYaw, 0f);

        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = GetPreviewBounds(renderers);
        previewObject.transform.position = Vector3.zero;
        previewPosition = Vector3.zero;
        previewDefaultDistance = CalculatePreviewDistance(bounds);
        previewDistance = previewDefaultDistance;
        previewNeedsRender = true;
        previewHasVisibleRenderer = HasVisibleRenderer(renderers);
        return previewHasVisibleRenderer;
    }

    private Bounds GetPreviewBounds() => GetPreviewBounds(previewObject.GetComponentsInChildren<Renderer>(true));

    private static Bounds GetPreviewBounds(Renderer[] renderers)
    {
        Bounds bounds = default;
        bool hasBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!IsRendererVisible(renderer))
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(renderer.bounds);
        }

        return hasBounds ? bounds : new Bounds(Vector3.zero, Vector3.one);
    }

    private static bool HasVisibleRenderer(Renderer[] renderers)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (IsRendererVisible(renderers[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsRendererVisible(Renderer renderer) => renderer.enabled && renderer.gameObject.activeSelf;

    private float CalculatePreviewDistance(Bounds bounds)
    {
        // Fit the bounds against each camera axis. A bounding sphere overestimates
        // the distance for thin or flat prefabs, making small assets look tiny.
        float aspect = previewTexture.width / (float)previewTexture.height;
        float verticalHalfFov = previewCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;
        float horizontalHalfFov = Mathf.Atan(Mathf.Tan(verticalHalfFov) * aspect);
        float horizontalDistance = bounds.extents.x / Mathf.Tan(horizontalHalfFov);
        float verticalDistance = bounds.extents.y / Mathf.Tan(verticalHalfFov);
        float distance = Mathf.Max(horizontalDistance, verticalDistance) * 1.05f + bounds.extents.z;
        return Mathf.Clamp(Mathf.Max(distance, MIN_PREVIEW_RADIUS), MIN_PREVIEW_DISTANCE, MAX_PREVIEW_DISTANCE);
    }

    private void EnsurePreviewCamera()
    {
        if (previewCamera && previewTexture)
        {
            return;
        }

        DestroyPreviewRenderingResources();

        GameObject cameraObject = new("[Prefab preview camera]");
        previewCamera = cameraObject.AddComponent<Camera>();
        previewCamera.clearFlags = CameraClearFlags.SolidColor;
        ApplyPreviewBackground();
        previewCamera.cullingMask = 1 << PREVIEW_LAYER;
        previewCamera.fieldOfView = 35f;
        previewCamera.nearClipPlane = 0.001f;
        previewCamera.enabled = false;
        previewTexture = new RenderTexture(640, 480, 16, RenderTextureFormat.ARGB32);
        previewTexture.Create();
        previewCamera.targetTexture = previewTexture;
    }

    private void ApplyPreviewBackground()
    {
        if (previewCamera)
        {
            previewCamera.backgroundColor = previewLightBackground ? Color.white : new Color(0.08f, 0.08f, 0.08f, 1f);
        }
    }

    private void RenderPreview()
    {
        if (!previewCamera || !previewObject)
        {
            return;
        }

        ApplyPreviewBackground();
        previewCamera.transform.position = new Vector3(0f, 0f, -previewDistance);
        previewCamera.transform.LookAt(Vector3.zero);
        previewObject.transform.rotation = Quaternion.Euler(previewPitch, previewYaw, 0f);
        previewObject.transform.position = Vector3.zero;
        Bounds rotatedBounds = GetPreviewBounds();
        previewObject.transform.position = previewPosition - rotatedBounds.center;
        // Render on demand only. Keeping the clone inactive between renders avoids
        // activating prefab behaviour or affecting the game scene.
        previewObject.SetActive(true);
        previewCamera.Render();
        previewObject.SetActive(false);
        previewNeedsRender = false;
    }

    private void HandlePreviewMouseInput(Rect previewRect)
    {
        Event currentEvent = Event.current;
        previewControlId = GUIUtility.GetControlID("PrefabPreview".GetHashCode(), FocusType.Passive, previewRect);
        // IMGUI does not automatically focus a texture, so the explicit control/hot
        // control pair is required to keep drag events inside the preview.
        if (currentEvent.type == EventType.ScrollWheel && previewRect.Contains(currentEvent.mousePosition))
        {
            previewDistance = Mathf.Clamp(previewDistance - currentEvent.delta.y * 0.25f, MIN_PREVIEW_DISTANCE, MAX_PREVIEW_DISTANCE);
            previewNeedsRender = true;
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.MouseDown && previewRect.Contains(currentEvent.mousePosition))
        {
            if (currentEvent.button == 0 || currentEvent.button == 1)
            {
                GUI.FocusControl("PrefabPreview");
                GUIUtility.hotControl = previewControlId;
                previewDragging = true;
                previewMoving = currentEvent.button == 0;
                previewMovingDepth = previewMoving && currentEvent.control;
                previewLastMousePosition = currentEvent.mousePosition;
                currentEvent.Use();
            }
        }
        else if (currentEvent.type == EventType.MouseDrag && previewDragging && GUIUtility.hotControl == previewControlId)
        {
            Vector2 delta = currentEvent.mousePosition - previewLastMousePosition;
            if (previewMoving)
            {
                if (previewMovingDepth)
                {
                    previewPosition.z = Mathf.Clamp(previewPosition.z - delta.y * 0.01f, -5f, 5f);
                }
                else
                {
                    previewPosition.x = Mathf.Clamp(previewPosition.x + delta.x * 0.01f, -5f, 5f);
                    previewPosition.y = Mathf.Clamp(previewPosition.y - delta.y * 0.01f, -5f, 5f);
                }
            }
            else
            {
                previewYaw = Mathf.Repeat(previewYaw + delta.x + 180f, 360f) - 180f;
                previewPitch = Mathf.Repeat(previewPitch - delta.y + 180f, 360f) - 180f;
            }
            previewLastMousePosition = currentEvent.mousePosition;
            previewNeedsRender = true;
            currentEvent.Use();
        }
        else if (currentEvent.type == EventType.MouseUp && previewDragging)
        {
            previewDragging = false;
            previewMoving = false;
            previewMovingDepth = false;
            if (GUIUtility.hotControl == previewControlId)
            {
                GUIUtility.hotControl = 0;
            }
        }
    }

    private static void DrawAxisGizmo(Rect previewRect)
    {
        const float PADDING = 8f;
        const float ORIGIN_OFFSET = AXIS_GIZMO_SIZE * 0.5f;
        Rect gizmoRect = new(previewRect.x + PADDING, previewRect.y + PADDING, AXIS_GIZMO_SIZE, AXIS_GIZMO_SIZE);
        GUI.Box(gizmoRect, GUIContent.none);

        Vector2 origin = new(gizmoRect.x + ORIGIN_OFFSET, gizmoRect.y + ORIGIN_OFFSET + 4f);
        DrawAxisLine(origin, new Vector2(22f, 8f), Color.red);
        DrawAxisLine(origin, new Vector2(0f, -24f), Color.green);
        DrawAxisLine(origin, new Vector2(-16f, 10f), Color.blue);

        GUI.Label(new Rect(origin.x + 22f, origin.y - 1f, 16f, 18f), "X", "axisLabelX");
        GUI.Label(new Rect(origin.x + 4f, origin.y - 39f, 16f, 18f), "Y", "axisLabelY");
        GUI.Label(new Rect(origin.x - 25f, origin.y + 7f, 16f, 18f), "Z", "axisLabelZ");
    }

    private static void DrawAxisLine(Vector2 origin, Vector2 direction, Color color)
    {
        float length = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Matrix4x4 originalMatrix = GUI.matrix;
        Color originalColor = GUI.color;

        try
        {
            GUIUtility.RotateAroundPivot(angle, origin);
            GUI.color = color;
            GUI.DrawTexture(new Rect(origin.x, origin.y - 1f, length, 2f), Texture2D.whiteTexture);
        }
        finally
        {
            GUI.matrix = originalMatrix;
            GUI.color = originalColor;
        }
    }

    private void InstantiateSelectedPrefab()
    {
        if (!loadedPrefab || !Player.main || !Player.main.viewModelCamera)
        {
            statusMessage = "A player, camera, and loaded prefab are required.";
            return;
        }

        // Instantiation is intentionally separate from the isolated preview clone.
        // Only objects created by this button are tracked for later cleanup.
        Vector3 forward = Player.main.viewModelCamera.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }
        forward.Normalize();
        Vector3 position = Player.main.transform.position + forward * 4f;
        GameObject spawnedObject = GameObjectExtensions.InstantiateWithId(loadedPrefab, new NitroxId(), position, Quaternion.LookRotation(forward, Vector3.up));
        spawnedObject.name = selectedClassId;
        spawnedObjects.Add(spawnedObject);
        statusMessage = $"Instantiated {selectedClassId}.";
    }

    private void DestroyPreview()
    {
        if (previewObject)
        {
            UnityEngine.Object.Destroy(previewObject);
            previewObject = null;
        }
        DestroyPreviewRenderingResources();
        previewHasVisibleRenderer = false;
        loadedPrefab = null;
    }

    private void DestroyPreviewRenderingResources()
    {
        if (previewCamera)
        {
            UnityEngine.Object.Destroy(previewCamera.gameObject);
            previewCamera = null;
        }
        if (previewTexture)
        {
            previewTexture.Release();
            UnityEngine.Object.Destroy(previewTexture);
            previewTexture = null;
        }
    }

    private static void SetLayerRecursively(Transform transform, int layer)
    {
        transform.gameObject.layer = layer;
        foreach (Transform child in transform)
        {
            SetLayerRecursively(child, layer);
        }
    }

    private static void DisablePreviewBehaviours(GameObject gameObject)
    {
        foreach (Behaviour behaviour in gameObject.GetComponentsInChildren<Behaviour>(true))
        {
            behaviour.enabled = false;
        }

        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        foreach (Rigidbody rigidbody in gameObject.GetComponentsInChildren<Rigidbody>(true))
        {
            rigidbody.isKinematic = true;
            rigidbody.detectCollisions = false;
        }
    }

    private static IEnumerable<string> GetPrefabClassIds()
    {
        return PrefabDatabase.prefabFiles.Keys
            .Where(classId => !string.IsNullOrWhiteSpace(classId))
            .OrderBy(classId => classId, StringComparer.OrdinalIgnoreCase);
    }

    private string GetPrefabName(string classId)
    {
        return prefabNameByClassId.TryGetValue(classId, out string? prefabName) ? prefabName : ResolvePrefabName(classId);
    }

    private static string ResolvePrefabName(string classId)
    {
        if (!PrefabDatabase.prefabFiles.TryGetValue(classId, out string? prefabPath))
        {
            return classId;
        }

        string prefabName = Path.GetFileNameWithoutExtension(prefabPath.Replace('/', Path.DirectorySeparatorChar));
        return string.IsNullOrWhiteSpace(prefabName) ? classId : prefabName;
    }

    private static string ResolveTechType(GameObject prefab)
    {
        TechType techType = CraftData.GetTechType(prefab);
        return techType == TechType.None ? UNKNOWN_TECH_TYPE : techType.AsString();
    }

    private string GetPrefabTechType(string classId)
    {
        return techTypeByClassId.TryGetValue(classId, out string? techType) ? techType : UNKNOWN_TECH_TYPE;
    }
}
