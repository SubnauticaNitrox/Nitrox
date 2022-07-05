using System;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class ColorDrawer : IStructDrawer
{
    public Type[] ApplicableTypes { get; } = { typeof(Color), typeof(Color32) };

    private static readonly Texture2D tex = new((int)NitroxGUILayout.VALUE_WIDTH, 25);
    private static Color lastColor;

    public object Draw(object target)
    {
        return target switch
        {
            Color color => Draw(color),
            Color32 color32 => Draw(color32),
            _ => null
        };
    }

    public static Color Draw(Color color)
    {
        if (color != lastColor)
        {
            for (int y = 0; y < 6; y++)
            {
                int separator = (int)(tex.width * color.a);
                for (int x = 0; x < separator; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }

                for (int x = separator; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, Color.black);
                }
            }

            for (int y = 6; y < tex.height; y++)
            {
                for (int x = 0; x < tex.width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            lastColor = color;
        }

        // TODO: Implement Color picker
        GUILayout.Button(tex, GUILayout.Width(tex.width));
        return color;
    }
}
