using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
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

    private const int PAGE_BUTTON_WIDTH = 100;
    private int searchPageIndex;
    private int resultsPerPage = 30;

    public override bool Enabled
    {
        get => base.Enabled;
        set
        {
            base.Enabled = value;
            if (value)
            {
                MoveOverlappingSceneDebugger();
            }
        }
    }

    public SceneExtraDebugger(SceneDebugger sceneDebugger, IMap map) : base(350, "Scene Tools", KeyCode.S, true, false, true, GUISkinCreationOptions.DERIVEDCOPY, 700)
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

        using (new GUILayout.VerticalScope("box", GUILayout.MinHeight(600)))
        {
            if (gameObjectResults.Count > 0)
            {
                GUILayout.Label($" Found {gameObjectResults.Count} results.");

                using (GUILayout.ScrollViewScope scroll = new(hierarchyScrollPos))
                {
                    hierarchyScrollPos = scroll.scrollPosition;

                    int startIndex = resultsPerPage * searchPageIndex;
                    int endIndex = startIndex + resultsPerPage;

                    if (endIndex > gameObjectResults.Count)
                    {
                        endIndex = gameObjectResults.Count;
                    }

                    for (int index = startIndex; index < endIndex; index++)
                    {
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

                // Needed to push the pagination buttons
                // down to the bottom when the scroll
                // view doesn't have enough height
                GUILayout.FlexibleSpace();

                // Pagination of search results if necessary
                if (gameObjectResults.Count > resultsPerPage)
                {
                    using (new GUILayout.HorizontalScope("box"))
                    {
                        // Only enable the back button if we can go back
                        GUI.enabled = searchPageIndex > 0;
                        if (GUILayout.Button("<", GUILayout.Width(PAGE_BUTTON_WIDTH)))
                        {
                            searchPageIndex--;
                            hierarchyScrollPos = Vector2.zero;
                            if (searchPageIndex < 0)
                            {
                                searchPageIndex = 0;
                            }
                        }
                        GUI.enabled = true;

                        // Get the maximum page number based on the size of the results
                        int maxPage = gameObjectResults.Count / resultsPerPage;

                        GUILayout.FlexibleSpace();
                        GUILayout.Label($"Page {searchPageIndex + 1} of {maxPage + 1}", GUILayout.ExpandHeight(true));
                        GUILayout.FlexibleSpace();

                        // Only enable the next button if we can go forward
                        GUI.enabled = maxPage > searchPageIndex;
                        if (GUILayout.Button(">", GUILayout.Width(PAGE_BUTTON_WIDTH)))
                        {
                            searchPageIndex++;
                            hierarchyScrollPos = Vector2.zero;
                            if (searchPageIndex > maxPage)
                            {
                                searchPageIndex = maxPage;
                            }
                        }

                        // Re-enable the GUI for anyone who comes after us
                        GUI.enabled = true;
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
                    searchPageIndex = 0;
                    hierarchyScrollPos = Vector2.zero;
                }

                if (GUILayout.Button("X", "button", GUILayout.Width(30)))
                {
                    gameObjectSearching = false;
                    gameObjectSearch = string.Empty;
                    gameObjectSearchCache = string.Empty;
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
                else if (gameObjectSearch.StartsWith("id:"))
                {
                    string id = gameObjectSearch.Split(':')[1];
                    try
                    {
                        NitroxId foundId = new(id);
                        if (NitroxEntity.TryGetObjectFrom(foundId, out GameObject gameObject))
                        {
                            gameObjectResults = [gameObject];
                        }
                        else
                        {
                            GUILayout.Label($"No GameObject found with NitroxId \"{foundId}\"");
                            gameObjectResults = [];
                        }
                    }
                    catch
                    {
                        GUILayout.Label($"Id \"{id}\" is not a valid NitroxId");
                        gameObjectResults = [];
                    }
                }
                else
                {
                    gameObjectResults = Resources.FindObjectsOfTypeAll<GameObject>().
                                        Where(go => Regex.IsMatch(go.name, gameObjectSearch, RegexOptions.IgnoreCase)).
                                        OrderBy(go => go.name).ToList();
                }

                gameObjectSearchCache = gameObjectSearch;
                searchPageIndex = 0;
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
        // Align to the right side of the SceneDebugger
        WindowRect.x = sceneDebugger.WindowRect.x + sceneDebugger.WindowRect.width;
        WindowRect.y = sceneDebugger.WindowRect.y;
        
        float exceedWidth = WindowRect.x + WindowRect.width - Screen.width;
        if (exceedWidth > 0f)
        {
            WindowRect.x -= exceedWidth;
        }
        MoveOverlappingSceneDebugger();
    }

    /// <summary>
    /// Move the scene debugger if it's overlapping with the extra scene debugger (if they can both hold in the available space)
    /// </summary>
    private void MoveOverlappingSceneDebugger()
    {
        if (sceneDebugger.WindowRect.width + WindowRect.width < Screen.width && // verify that debuggers can hold at the same time in the screen
            sceneDebugger.WindowRect.x + sceneDebugger.WindowRect.width + WindowRect.width > Screen.width) // verify that debuggers are really overlapping
        {
            sceneDebugger.WindowRect.x = Screen.width - WindowRect.width - sceneDebugger.WindowRect.width;
        }
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
