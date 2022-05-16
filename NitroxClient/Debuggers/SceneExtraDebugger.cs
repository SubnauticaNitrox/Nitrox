using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using UnityEngine;
using Mathf = UnityEngine.Mathf;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public sealed class SceneExtraDebugger : BaseDebugger
{
    private readonly SceneDebugger sceneDebugger;
    private readonly IMap map;

    private const KeyCode RAY_CAST_KEY = KeyCode.F9;

    private bool worldMarkerEnabled = true;
    private bool rayCastingEnabled;

    private string gameObjectSearch = string.Empty;
    private string gameObjectSearchCache = string.Empty;
    private bool gameObjectSearching;
    private string gameObjectSearchPatternInvalidMessage = string.Empty;
    private List<GameObject> gameObjectResults = new();

    private Vector2 hierarchyScrollPos;

    private readonly Lazy<Texture> arrowTexture, circleTexture;

    public SceneExtraDebugger(SceneDebugger sceneDebugger, IMap map) : base(350, "Scene Tools", KeyCode.S, true, false, true, GUISkinCreationOptions.DERIVEDCOPY)
    {
        this.sceneDebugger = sceneDebugger;
        this.map = map;
        ActiveTab = AddTab("Tools", RenderTabTools);

        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        circleTexture = new Lazy<Texture>(() => Resources.Load<Material>("Materials/WorldCursor").GetTexture("_MainTex"));
        arrowTexture = new Lazy<Texture>(() => Resources.Load<Texture2D>("Sprites/Arrow"));

        ResetWindowPosition();
    }

    public override void OnGUI()
    {
        base.OnGUI();
        if (worldMarkerEnabled && sceneDebugger.SelectedObject)
        {
            UpdateSelectedObjectMarker(sceneDebugger.SelectedObject.transform);
        }
    }

    private void RenderTabTools()
    {
        using (new GUILayout.HorizontalScope("box"))
        {
            if (GUILayout.Button($"World Marker: {(worldMarkerEnabled ? "Active" : "Inactive")}"))
            {
                worldMarkerEnabled = !worldMarkerEnabled;
            }

            if (GUILayout.Button($"Ray Casting: {(rayCastingEnabled ? "Active" : "Inactive")}"))
            {
                Log.InGame($"Ray casting can be enabled/disabled with: {RAY_CAST_KEY}");
            }
        }

        GettingRayCastResults();
        GettingSearchbarResults();

        using (new GUILayout.VerticalScope("box", GUILayout.MinHeight(425)))
        {
            if (gameObjectResults.Count > 0)
            {
                using GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos);
                hierarchyScrollPos = scroll.scrollPosition;

                for (int index = 0; index < gameObjectResults.Count; index++)
                {
                    if (index > 30)
                    {
                        GUILayout.Label($"There are {gameObjectResults.Count - index} more results which aren't displayed", "fillMessage");
                        break;
                    }

                    GameObject child = gameObjectResults[index];
                    if (child)
                    {
                        using (new GUILayout.VerticalScope("box"))
                        {
                            if (GUILayout.Button(child.GetFullHierarchyPath(), child.transform.childCount > 0 ? "bold" : "label"))
                            {
                                sceneDebugger.UpdateSelectedObject(child);
                            }
                        }
                    }
                }
            }
            else
            {
                GUILayout.Label("No results", "fillMessage");
            }
        }
    }

    private void GettingSearchbarResults()
    {
        using (new GUILayout.HorizontalScope("box"))
        {
            if (rayCastingEnabled)
            {
                GUILayout.TextField(gameObjectSearch);
                return;
            }

            gameObjectSearch = GUILayout.TextField(gameObjectSearch);

            // Disable searching if text is cleared after a search has happened.
            if (gameObjectSearching && string.IsNullOrEmpty(gameObjectSearch))
            {
                gameObjectSearching = false;
            }

            if (gameObjectSearch.Length > 0)
            {
                if (GUILayout.Button("Search", "button", GUILayout.Width(80)))
                {
                    gameObjectSearching = true;
                }

                if (GUILayout.Button("X", "button", GUILayout.Width(30)))
                {
                    gameObjectSearching = false;
                    gameObjectSearch = string.Empty;
                    gameObjectResults.Clear();
                }
                else if (Event.current.isKey && Event.current.keyCode == KeyCode.Return)
                {
                    gameObjectSearching = true;
                }
            }
        }

        // Searching. Return all gameobjects with matching type name.
        if (gameObjectSearching && gameObjectSearch != gameObjectSearchCache && gameObjectSearch.Length > 2)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Regex.IsMatch(string.Empty, gameObjectSearch);
                gameObjectSearchPatternInvalidMessage = string.Empty;
            }
            catch (Exception ex)
            {
                gameObjectSearchPatternInvalidMessage = ex.Message;
            }

            if (string.IsNullOrEmpty(gameObjectSearchPatternInvalidMessage))
            {
                if (gameObjectSearch.StartsWith("t:"))
                {
                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                                         .Select(a => a.GetType(gameObjectSearch.Substring(2), false, true))
                                         .FirstOrDefault(t => t != null);
                    if (type != null)
                    {
                        List<GameObject> gameObjects = Resources.FindObjectsOfTypeAll<GameObject>()
                                                                .Where(g => g.GetComponent(type))
                                                                .ToList();
                        gameObjectResults = gameObjects;
                    }
                    else
                    {
                        GUILayout.Label($"There is no component named \"{gameObjectSearch.Substring(2)}\"", "error");
                    }
                }
                else
                {
                    gameObjectResults = Resources.FindObjectsOfTypeAll<GameObject>().Where(go => Regex.IsMatch(go.name, gameObjectSearch, RegexOptions.IgnoreCase)).OrderBy(go => go.name).ToList();
                }

                gameObjectSearchCache = gameObjectSearch;
            }
            else
            {
                GUILayout.Label(gameObjectSearchPatternInvalidMessage, "error");
            }
        }
    }

    private void GettingRayCastResults()
    {
        if (Input.GetKeyDown(RAY_CAST_KEY))
        {
            gameObjectSearching = false;
            rayCastingEnabled = !rayCastingEnabled;

            gameObjectSearch = rayCastingEnabled ? "Ray casting is running" : string.Empty;
        }

        if (rayCastingEnabled)
        {
            gameObjectResults.Clear();

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, map.DimensionsInMeters.X, int.MaxValue);

            foreach (RaycastHit hit in hits)
            {
                GameObject hitObject = hit.transform.gameObject;
                if (gameObjectResults.Contains(hitObject) || hitObject == Player.main.gameObject) // We want to ignore the player because of the buoyancy results in flickering of the entry
                {
                    continue;
                }

                gameObjectResults.Add(hitObject);
            }
        }
    }

    public override void ResetWindowPosition()
    {
        base.ResetWindowPosition();
        WindowRect.x += sceneDebugger.WindowRect.width / 2f + WindowRect.width / 2f; // Altered position to align on the right side of the SceneDebugger
        WindowRect.y = sceneDebugger.WindowRect.y;
    }

    private void UpdateSelectedObjectMarker(Transform selectedTransform)
    {
        if (!Player.main || !Player.main.viewModelCamera || !Multiplayer.Active) // Only works in game
        {
            return;
        }

        Texture currentTexture;
        float markerX, markerY, markerRot;

        Vector3 screenPos = Player.main.viewModelCamera.WorldToScreenPoint(selectedTransform.position);

        //if object is on screen
        if (screenPos.x >= 0 && screenPos.x < Screen.width &&
            screenPos.y >= 0 && screenPos.y < Screen.height &&
            screenPos.z > 0)
        {
            currentTexture = circleTexture.Value;
            markerX = screenPos.x;
            //subtract from height to go from bottom up to top down
            markerY = Screen.height - screenPos.y;
            markerRot = 0;
        }
        else // If object is not on screen
        {
            currentTexture = arrowTexture.Value;
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

    protected override void OnSetSkin(GUISkin skin)
    {
        base.OnSetSkin(skin);
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
        skin.SetCustomStyle("fillMessage", skin.label, s =>
        {
            s.stretchWidth = true;
            s.stretchHeight = true;
            s.fontSize = 24;
            s.alignment = TextAnchor.MiddleCenter;
            s.fontStyle = FontStyle.Italic;
        });
    }
}
