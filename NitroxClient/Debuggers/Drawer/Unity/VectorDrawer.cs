using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class VectorDrawer : IStructDrawer
{
    private const float MAX_WIDTH = 400;

    public Type[] ApplicableTypes { get; } = { typeof(Vector2), typeof(Vector3), typeof(NitroxVector3), typeof(Vector4), typeof(NitroxVector4), typeof(Quaternion), typeof(Int3) };

    public object Draw(object target)
    {
        return target switch
        {
            Vector2 vector2 => DrawVector2(vector2),
            Vector3 vector3 => DrawVector3(vector3),
            NitroxVector3 nitroxVector3 => DrawVector3(nitroxVector3.ToUnity()).ToDto(),
            Vector4 vector4 => DrawVector4(vector4),
            NitroxVector4 nitroxVector4 => DrawVector4(nitroxVector4.ToUnity()).ToDto(),
            Quaternion quaternion => DrawQuaternion(quaternion),
            Int3 int3 => DrawInt3(int3),
            _ => null
        };
    }

    public static Vector2 DrawVector2(Vector2 vector2, float maxWidth = MAX_WIDTH)
    {
        float valueWidth = maxWidth / 2 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            vector2.x = NitroxGUILayout.FloatField(vector2.x, valueWidth);
            NitroxGUILayout.Separator();
            vector2.y = NitroxGUILayout.FloatField(vector2.y, valueWidth);
            return vector2;
        }
    }

    public static Vector3 DrawVector3(Vector3 vector3, float maxWidth = MAX_WIDTH)
    {
        float valueWidth = maxWidth / 3 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            vector3.x = NitroxGUILayout.FloatField(vector3.x, valueWidth);
            NitroxGUILayout.Separator();
            vector3.y = NitroxGUILayout.FloatField(vector3.y, valueWidth);
            NitroxGUILayout.Separator();
            vector3.z = NitroxGUILayout.FloatField(vector3.z, valueWidth);
            return vector3;
        }
    }

    public static Vector4 DrawVector4(Vector4 vector4, float maxWidth = MAX_WIDTH)
    {
        float valueWidth = maxWidth / 4 - 6;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            vector4.x = NitroxGUILayout.FloatField(vector4.x, valueWidth);
            NitroxGUILayout.Separator();
            vector4.y = NitroxGUILayout.FloatField(vector4.y, valueWidth);
            NitroxGUILayout.Separator();
            vector4.z = NitroxGUILayout.FloatField(vector4.z, valueWidth);
            NitroxGUILayout.Separator();
            vector4.w = NitroxGUILayout.FloatField(vector4.w, valueWidth);
            return vector4;
        }
    }

    public static Quaternion DrawQuaternion(Quaternion quaternion, float maxWidth = MAX_WIDTH)
    {
        Vector4 vector4 = new(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
        vector4 = DrawVector4(vector4, maxWidth);
        return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
    }

    public static Int3 DrawInt3(Int3 int3, float maxWidth = MAX_WIDTH)
    {
        float valueWidth = maxWidth / 3 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            int3.x = NitroxGUILayout.IntField(int3.x, valueWidth);
            NitroxGUILayout.Separator();
            int3.y = NitroxGUILayout.IntField(int3.y, valueWidth);
            NitroxGUILayout.Separator();
            int3.z = NitroxGUILayout.IntField(int3.z, valueWidth);
            return int3;
        }
    }

    public static Tuple<int, int, int, int> DrawInt4(int item1, int item2, int item3, int item4, float maxWidth = MAX_WIDTH)
    {
        float valueWidth = maxWidth / 4 - 6;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(maxWidth)))
        {
            item1 = NitroxGUILayout.IntField(item1, valueWidth);
            NitroxGUILayout.Separator();
            item2 = NitroxGUILayout.IntField(item2, valueWidth);
            NitroxGUILayout.Separator();
            item4 = NitroxGUILayout.IntField(item4, valueWidth);
            NitroxGUILayout.Separator();
            item4 = NitroxGUILayout.IntField(item4, valueWidth);
            return new Tuple<int, int, int, int>(item1, item2, item3, item4);
        }
    }
}
