using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
/// Renders synchronized remote-player bearings on the native compass without changing vanilla ping behavior.
/// </summary>
internal sealed class RemotePlayerCompass : MonoBehaviour
{
    private const float MINIMUM_PROJECTION_DEPTH = 0.000001f;
    private static readonly Color outlineColor = new(0f, 0f, 0f, 0.8f);

    private readonly Dictionary<SessionId, MarkerInstance> markersBySessionId = [];
    private PlayerManager playerManager = null!;
    private uGUI_Compass compass = null!;
    private RectTransform markerParent = null!;
    private RectTransform markerRoot = null!;

    private void Awake()
    {
        playerManager = this.Resolve<PlayerManager>();
    }

    private void OnEnable()
    {
        ManagedUpdate.Subscribe(ManagedUpdate.Queue.PreCanvasLast, RefreshMarkers);
    }

    private void OnDisable()
    {
        ManagedUpdate.Unsubscribe(ManagedUpdate.Queue.PreCanvasLast, RefreshMarkers);
        SetMarkerRootActive(false);
    }

    private void OnDestroy()
    {
        ReleaseCompass();
    }

    private void RefreshMarkers()
    {
        if (!TryBindCompass())
        {
            return;
        }

        bool shouldShowMarkers = compass._visible &&
                                 compass.isActiveAndEnabled &&
                                 compass.gameObject.activeInHierarchy &&
                                 Player.main &&
                                 markerParent &&
                                 markerRoot;
        SetMarkerRootActive(shouldShowMarkers);
        if (!shouldShowMarkers)
        {
            return;
        }

        List<RemotePlayer> remotePlayers = playerManager.GetAll().ToList();
        HashSet<SessionId> connectedSessionIds = new(remotePlayers.Select(player => player.SessionId));
        RemoveDisconnectedMarkers(connectedSessionIds);

        Vector3 observerWorldPosition = Player.main.transform.position;
        float compassHeadingDegrees = compass.direction * 360f;
        Dictionary<SessionId, RemotePlayer> visiblePlayersBySessionId = [];
        List<RemotePlayerCompassLayoutInput> layoutInputs = [];

        foreach (RemotePlayer remotePlayer in remotePlayers.OrderBy(player => player.SessionId))
        {
            if (!remotePlayer.Body || !remotePlayer.Body.activeInHierarchy ||
                !RemotePlayerCompassProjector.TryProject(
                    remotePlayer.SessionId,
                    observerWorldPosition,
                    remotePlayer.Body.transform.position,
                    compassHeadingDegrees,
                    compass.alphaFrom,
                    compass.alphaTo,
                    out RemotePlayerCompassProjection projection) ||
                !TryProjectBearingToMarkerParent(projection.DisplayBearingDegrees, out Vector2 basePosition))
            {
                continue;
            }

            visiblePlayersBySessionId[remotePlayer.SessionId] = remotePlayer;
            layoutInputs.Add(new RemotePlayerCompassLayoutInput(remotePlayer.SessionId, projection.MarkerKind, basePosition));
        }

        foreach (MarkerInstance marker in markersBySessionId
                                                .Where(pair => !visiblePlayersBySessionId.ContainsKey(pair.Key))
                                                .Select(pair => pair.Value))
        {
            if (marker.GameObject)
            {
                marker.GameObject.SetActive(false);
            }
        }

        foreach (RemotePlayerCompassLayoutResult layout in RemotePlayerCompassMarkerLayout.Arrange(layoutInputs))
        {
            RemotePlayer remotePlayer = visiblePlayersBySessionId[layout.SessionId];
            MarkerInstance marker = GetOrCreateMarker(layout.SessionId, remotePlayer.PlayerName);
            Color markerColor = remotePlayer.PlayerSettings.PlayerColor.ToUnity();
            marker.Graphic.Configure(layout.MarkerKind, markerColor);
            marker.RectTransform.localPosition = new Vector3(layout.Position.x, layout.Position.y, 0f);
            marker.GameObject.SetActive(true);
        }
    }

    private bool TryBindCompass()
    {
        if (compass && markerParent && markerRoot)
        {
            return true;
        }

        ReleaseCompass();
        uGUI_DepthCompass depthCompass = FindObjectOfType<uGUI_DepthCompass>();
        if (!depthCompass || !depthCompass.compass)
        {
            return false;
        }

        RectTransform labelParent = depthCompass.compass.labels?
                                                  .Where(label => label?.text)
                                                  .Select(label => label.text.rectTransform.parent as RectTransform)
                                                  .FirstOrDefault(parent => parent);
        if (!labelParent)
        {
            return false;
        }

        compass = depthCompass.compass;
        markerParent = labelParent;

        GameObject markerRootObject = new("Nitrox remote-player compass markers", typeof(RectTransform));
        markerRootObject.layer = compass.gameObject.layer;
        markerRoot = markerRootObject.GetComponent<RectTransform>();
        markerRoot.SetParent(markerParent, false);
        markerRoot.localPosition = Vector3.zero;
        markerRoot.localRotation = Quaternion.identity;
        markerRoot.localScale = Vector3.one;
        markerRoot.SetAsLastSibling();
        return true;
    }

    private bool TryProjectBearingToMarkerParent(float bearingDegrees, out Vector2 position)
    {
        position = default;
        if (!IsFinite(bearingDegrees) || !compass || !markerParent)
        {
            return false;
        }

        float angleRadians = (compass.rotation.z + bearingDegrees) * Mathf.Deg2Rad;
        Vector3 compassPoint = new(
            compass.radius * Mathf.Cos(angleRadians),
            compass.radius * Mathf.Sin(angleRadians),
            0f);
        Matrix4x4 compassMatrix = compass.projection * Matrix4x4.TRS(
            compass.position,
            Quaternion.Euler(compass.rotation.x, compass.rotation.y, compass.rotation.z - compass.direction * 360f),
            compass.scale);
        Vector3 projectedPoint = compassMatrix.MultiplyPoint3x4(compassPoint);
        if (!IsFinite(projectedPoint) || Mathf.Abs(projectedPoint.z) <= MINIMUM_PROJECTION_DEPTH)
        {
            return false;
        }

        Vector3 compassLocalPoint = new(
            compass.scale2D * projectedPoint.x / projectedPoint.z,
            compass.scale2D * projectedPoint.y / projectedPoint.z,
            0f);
        Vector3 worldPoint = compass.transform.localToWorldMatrix.MultiplyPoint3x4(compassLocalPoint);
        Vector3 parentLocalPoint = markerParent.worldToLocalMatrix.MultiplyPoint3x4(worldPoint);
        if (!IsFinite(parentLocalPoint))
        {
            return false;
        }

        position = parentLocalPoint;
        return true;
    }

    private MarkerInstance GetOrCreateMarker(SessionId sessionId, string playerName)
    {
        if (markersBySessionId.TryGetValue(sessionId, out MarkerInstance existingMarker) && existingMarker.GameObject)
        {
            existingMarker.GameObject.name = $"Nitrox compass marker - {playerName}";
            return existingMarker;
        }

        GameObject markerObject = new($"Nitrox compass marker - {playerName}", typeof(RectTransform), typeof(CanvasRenderer), typeof(RemotePlayerCompassMarkerGraphic));
        markerObject.layer = markerRoot.gameObject.layer;
        RectTransform rectTransform = markerObject.GetComponent<RectTransform>();
        rectTransform.SetParent(markerRoot, false);
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        RemotePlayerCompassMarkerGraphic graphic = markerObject.GetComponent<RemotePlayerCompassMarkerGraphic>();
        Outline outline = markerObject.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);
        outline.useGraphicAlpha = true;

        MarkerInstance marker = new(markerObject, rectTransform, graphic);
        markersBySessionId[sessionId] = marker;
        return marker;
    }

    private void RemoveDisconnectedMarkers(ISet<SessionId> connectedSessionIds)
    {
        foreach (SessionId disconnectedSessionId in markersBySessionId.Keys
                                                                          .Where(sessionId => !connectedSessionIds.Contains(sessionId))
                                                                          .ToList())
        {
            MarkerInstance marker = markersBySessionId[disconnectedSessionId];
            if (marker.GameObject)
            {
                Destroy(marker.GameObject);
            }
            markersBySessionId.Remove(disconnectedSessionId);
        }
    }

    private void ReleaseCompass()
    {
        if (markerRoot)
        {
            markerRoot.gameObject.SetActive(false);
            Destroy(markerRoot.gameObject);
        }

        markersBySessionId.Clear();
        compass = null!;
        markerParent = null!;
        markerRoot = null!;
    }

    private void SetMarkerRootActive(bool active)
    {
        if (markerRoot && markerRoot.gameObject.activeSelf != active)
        {
            markerRoot.gameObject.SetActive(active);
        }
    }

    private static bool IsFinite(Vector3 value) => IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);

    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private sealed record MarkerInstance(
        GameObject GameObject,
        RectTransform RectTransform,
        RemotePlayerCompassMarkerGraphic Graphic);
}
