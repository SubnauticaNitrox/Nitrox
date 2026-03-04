using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nitrox.Model.DataStructures;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Debuggers;

[ExcludeFromCodeCoverage]
public class EntityDebugger : AbstractDebugger
{
    private const int WINDOW_ID = 421;
    private const float TEXT_X_OFFSET = 20f;

    private static Color labelBgColor = new(0.2f, 0.2f, 0.2f, 0.5f);
    private static Color labelFgColor = Color.white;
    private static Texture2D? lineTex;

    private static readonly List<Rect> rects = [];

    public EntityDebugger() : base(WINDOW_ID, 200, null, KeyCode.E, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
    {
        ActiveTab = AddTab("EntityDebugger", RenderEntityDebugger);
    }

    private void RenderEntityDebugger()
    {

    }

    public override void OnGUI()
    {
        if (!Enabled)
        {
            return;
        }

        rects.Clear();
        foreach (KeyValuePair<NitroxId, GameObject> gameObjectPairs in NitroxEntity.GetGameObjects())
        {
            NitroxId id = gameObjectPairs.Key;
            GameObject gameObject = gameObjectPairs.Value;
            if (gameObject == null || gameObject == Player.mainObject)
            {
                continue;
            }

            Vector3 screenPos = Player.main.viewModelCamera.WorldToScreenPoint(gameObject.transform.position);
            if (screenPos.z > 0 && screenPos.z < 20 &&
                screenPos.x >= 0 && screenPos.x < Screen.width &&
                screenPos.y >= 0 && screenPos.y < Screen.height)
            {
                GUIStyle style = GUI.skin.label;
                GUIContent textContent = new($"ID {id}   NAME {gameObject.name}");
                Vector2 size = style.CalcSize(textContent);
                size += new Vector2(10f, 0f); //for box edges

                Vector2 pointLocation = new(screenPos.x, Screen.height - screenPos.y);
                Rect drawSize = new(screenPos.x + TEXT_X_OFFSET, Screen.height - screenPos.y, size.x, size.y);
                while (true)
                {
                    bool finished = true;
                    foreach (Rect rect in rects)
                    {
                        if (rect.Overlaps(drawSize))
                        {
                            drawSize.x = rect.x;
                            drawSize.y = rect.y + rect.height;
                            finished = false;
                            break;
                        }
                    }
                    if (finished)
                    {
                        break;
                    }
                }

                DrawLine(pointLocation, new Vector2(drawSize.x, drawSize.y + size.y / 2), labelFgColor, 2f);

                rects.Add(drawSize);

                Color oldBgColor = GUI.backgroundColor;
                GUI.backgroundColor = labelBgColor;
                GUI.color = labelFgColor;
                GUI.Box(drawSize, textContent);

                GUI.backgroundColor = oldBgColor;
            }
        }
    }

    private static void DrawLine(Vector2 pointA, Vector2 pointB, Color color, float width)
    {
        Matrix4x4 matrix = GUI.matrix;

        if (!lineTex)
        {
            lineTex = new Texture2D(1, 1);
        }

        Color savedColor = GUI.color;
        GUI.color = color;

        float angle = Vector3.Angle(pointB - pointA, Vector2.right);

        if (pointA.y > pointB.y)
        {
            angle = -angle;
        }

        GUIUtility.ScaleAroundPivot(new Vector2((pointB - pointA).magnitude, width), new Vector2(pointA.x, pointA.y + 0.5f));

        GUIUtility.RotateAroundPivot(angle, pointA);

        GUI.DrawTexture(new Rect(pointA.x, pointA.y, 1, 1), lineTex);

        GUI.matrix = matrix;
        GUI.color = savedColor;
    }
}
