using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
/// Draws the emote wheel as resolution-independent UI geometry.
/// </summary>
internal sealed class EmoteWheelGraphic : MaskableGraphic
{
    private const int SEGMENT_COUNT = 9;
    private const int ARC_STEPS = 8;
    private const float SEGMENT_ANGLE = 360f / SEGMENT_COUNT;
    private const float SEGMENT_GAP = 4f;
    private const float OUTER_RADIUS = 280f;
    private const float INNER_RADIUS = 105f;

    private static readonly Color32 abyssGlass = new(5, 19, 29, 222);
    private static readonly Color32 segmentCyan = new(13, 74, 94, 205);
    private static readonly Color32 segmentBorder = new(100, 218, 238, 92);
    private static readonly Color32 recentCyan = new(65, 217, 255, 255);
    private static readonly Color32 selectedOrange = new(255, 156, 66, 248);
    private static readonly Color32 selectedNeedle = new(255, 190, 112, 236);

    private int recentSegment;
    private int selectedSegment = -1;
    private float selectionAngle;

    public void SetState(int selectedIndex, int recentIndex, float rawSelectionAngle)
    {
        selectedSegment = selectedIndex;
        recentSegment = recentIndex;
        selectionAngle = rawSelectionAngle;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();
        Vector2 center = rectTransform.rect.center;

        AddDisc(vertexHelper, center, INNER_RADIUS - 14f, abyssGlass, 48);
        AddRingSector(vertexHelper, center, INNER_RADIUS - 4f, INNER_RADIUS, 0f, 360f, segmentBorder, 48);

        for (int index = 0; index < SEGMENT_COUNT; index++)
        {
            float centerAngle = 90f - index * SEGMENT_ANGLE;
            float halfVisibleAngle = (SEGMENT_ANGLE - SEGMENT_GAP) * 0.5f;
            bool selected = index == selectedSegment;
            float innerRadius = selected ? INNER_RADIUS - 3f : INNER_RADIUS;
            float outerRadius = selected ? OUTER_RADIUS + 8f : OUTER_RADIUS;
            Color32 fillColor = selected ? selectedOrange : segmentCyan;

            AddRingSector(vertexHelper, center, innerRadius, outerRadius, centerAngle - halfVisibleAngle, centerAngle + halfVisibleAngle, fillColor, ARC_STEPS);
            AddRingSector(vertexHelper, center, outerRadius - 3f, outerRadius, centerAngle - halfVisibleAngle, centerAngle + halfVisibleAngle, selected ? selectedNeedle : segmentBorder, ARC_STEPS);

            if (index == recentSegment && !selected)
            {
                AddRingSector(vertexHelper, center, OUTER_RADIUS + 5f, OUTER_RADIUS + 11f, centerAngle - 8f, centerAngle + 8f, recentCyan, 4);
            }
        }

        if (selectedSegment >= 0)
        {
            AddRingSector(vertexHelper, center, 48f, INNER_RADIUS - 12f, selectionAngle - 1.25f, selectionAngle + 1.25f, selectedNeedle, 1);
        }
    }

    private static void AddDisc(VertexHelper vertexHelper, Vector2 center, float radius, Color32 color, int steps)
    {
        int centerVertex = vertexHelper.currentVertCount;
        vertexHelper.AddVert(center, color, Vector2.zero);

        for (int index = 0; index <= steps; index++)
        {
            float angle = index * 360f / steps * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            vertexHelper.AddVert(center + direction * radius, color, Vector2.zero);
        }

        for (int index = 0; index < steps; index++)
        {
            vertexHelper.AddTriangle(centerVertex, centerVertex + index + 1, centerVertex + index + 2);
        }
    }

    private static void AddRingSector(
        VertexHelper vertexHelper,
        Vector2 center,
        float innerRadius,
        float outerRadius,
        float startAngle,
        float endAngle,
        Color32 color,
        int steps)
    {
        int firstVertex = vertexHelper.currentVertCount;
        for (int index = 0; index <= steps; index++)
        {
            float angle = Mathf.Lerp(startAngle, endAngle, index / (float)steps) * Mathf.Deg2Rad;
            Vector2 direction = new(Mathf.Cos(angle), Mathf.Sin(angle));
            vertexHelper.AddVert(center + direction * innerRadius, color, Vector2.zero);
            vertexHelper.AddVert(center + direction * outerRadius, color, Vector2.zero);
        }

        for (int index = 0; index < steps; index++)
        {
            int innerStart = firstVertex + index * 2;
            int outerStart = innerStart + 1;
            int innerEnd = innerStart + 2;
            int outerEnd = innerStart + 3;
            vertexHelper.AddTriangle(innerStart, outerStart, outerEnd);
            vertexHelper.AddTriangle(innerStart, outerEnd, innerEnd);
        }
    }
}
