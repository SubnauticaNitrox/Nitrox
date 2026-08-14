using NitroxClient.GameLogic.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

internal sealed class RemotePlayerCompassMarkerGraphic : Graphic
{
    internal const float BlipSize = 10f;
    internal static readonly Vector2 ArrowSize = new(12f, 10f);
    private const int CircleSegmentCount = 16;

    private RemotePlayerCompassMarkerKind markerKind;

    internal void Configure(RemotePlayerCompassMarkerKind newMarkerKind, Color markerColor)
    {
        markerColor.a = 1f;
        color = markerColor;
        raycastTarget = false;

        if (markerKind == newMarkerKind && rectTransform.sizeDelta != Vector2.zero)
        {
            return;
        }

        markerKind = newMarkerKind;
        rectTransform.sizeDelta = markerKind == RemotePlayerCompassMarkerKind.Blip
            ? new Vector2(BlipSize, BlipSize)
            : ArrowSize;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Color32 markerColor = color;

        switch (markerKind)
        {
            case RemotePlayerCompassMarkerKind.Blip:
                AddCircle(vertexHelper, markerColor);
                break;
            case RemotePlayerCompassMarkerKind.LeftArrow:
                AddArrow(vertexHelper, markerColor, pointsRight: false);
                break;
            case RemotePlayerCompassMarkerKind.RightArrow:
                AddArrow(vertexHelper, markerColor, pointsRight: true);
                break;
        }
    }

    private static void AddCircle(VertexHelper vertexHelper, Color32 markerColor)
    {
        float radius = BlipSize * 0.5f;
        AddVertex(vertexHelper, Vector2.zero, markerColor);
        for (int index = 0; index <= CircleSegmentCount; index++)
        {
            float angle = index * Mathf.PI * 2f / CircleSegmentCount;
            AddVertex(vertexHelper, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius, markerColor);
        }

        for (int index = 0; index < CircleSegmentCount; index++)
        {
            vertexHelper.AddTriangle(0, index + 1, index + 2);
        }
    }

    private static void AddArrow(VertexHelper vertexHelper, Color32 markerColor, bool pointsRight)
    {
        float halfWidth = ArrowSize.x * 0.5f;
        float halfHeight = ArrowSize.y * 0.5f;
        float direction = pointsRight ? 1f : -1f;

        AddVertex(vertexHelper, new Vector2(direction * halfWidth, 0f), markerColor);
        AddVertex(vertexHelper, new Vector2(-direction * halfWidth, halfHeight), markerColor);
        AddVertex(vertexHelper, new Vector2(-direction * halfWidth, -halfHeight), markerColor);
        vertexHelper.AddTriangle(0, 1, 2);
    }

    private static void AddVertex(VertexHelper vertexHelper, Vector2 position, Color32 markerColor)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.position = position;
        vertex.color = markerColor;
        vertexHelper.AddVert(vertex);
    }
}
